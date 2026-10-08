using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using RepositorioRemoto.Cache.Common;

public class UserService(
    IRepository _repository,
    IUserApiService _api,
    IUnitOfWork _unitOfWork,
    ICache<UserModel> _cache,
    ILogger<UserService> _logger
) : IUserService
{
    /// <summary>
    /// Sincronizar los usuarios de la API con la base de datos local.
    /// </summary>
    public async Task<Result<int, DomainError>> SyncUsersAsync(
        CancellationToken cts = default)
    {
        _logger.LogInformation(
            "Iniciando la sincronización de usuarios.");

        var apiResult = await _api.GetAllAsync(cts);

        if (apiResult.IsFailure)
        {
            _logger.LogError(
                "No se pudo obtener los usuarios de la API. Se cancela la sincronización.");

            return Result.Failure<int, DomainError>(
                apiResult.Error);
        }

        _logger.LogInformation(
            "Se obtuvieron {TotalUsers} usuarios de la API.",
            apiResult.Value.Count);

        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var result = await SyncDatabaseAsync(
                apiResult.Value.Select(u => u.ToDto()).ToList(),
                cts);

            if (result.IsSuccess)
            {
                return result;
            }

            if (attempt == maxAttempts)
            {
                _logger.LogError(
                    "La sincronización falló después de {MaxAttempts} intentos.",
                    maxAttempts);

                return result;
            }

            _logger.LogWarning(
                "Error durante la sincronización. Reintentando ({Attempt}/{MaxAttempts})...",
                attempt,
                maxAttempts);

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                cts);
        }

        return Result.Failure<int, DomainError>(
            new DatabaseError.Unknown(
                "No se pudo completar la sincronización."));
    }

    private async Task<Result<int, DomainError>> SyncDatabaseAsync(
        List<UserDto> users,
        CancellationToken cts)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cts);

            _logger.LogDebug(
                "Transacción de sincronización iniciada.");

            var deleteResult =
                await _repository.DeleteAllAsync();

            if (deleteResult.IsFailure)
            {
                _logger.LogWarning(
                    "No se pudo limpiar la base de datos.");

                await _unitOfWork.RollbackTransactionAsync(
                    CancellationToken.None);

                return Result.Failure<int, DomainError>(
                    deleteResult.Error);
            }

            _logger.LogDebug(
                "Base de datos limpiada correctamente.");

            var clearCacheResult =
                await _cache.ClearAsync();

            if (clearCacheResult.IsFailure)
            {
                _logger.LogWarning(
                    "No se pudo limpiar la caché.");

                await _unitOfWork.RollbackTransactionAsync(
                    CancellationToken.None);

                return Result.Failure<int, DomainError>(
                    clearCacheResult.Error);
            }

            _logger.LogDebug(
                "Caché limpiada correctamente.");

            var batchSize = AppConfig.BatchSettings.BatchSize;
            var totalInserted = 0;

            foreach (var batch in users.Chunk(batchSize))
            {
                cts.ThrowIfCancellationRequested();

                var entities = batch
                    .Select(u => u.ToModel().ToEntity())
                    .ToList();

                var createResult =
                    await _repository.CreateRangeAsync(entities);

                if (createResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Error al insertar el lote de usuarios. Tamaño: {BatchSize}.",
                        entities.Count);

                    await _unitOfWork.RollbackTransactionAsync(
                        CancellationToken.None);

                    return Result.Failure<int, DomainError>(
                        createResult.Error);
                }

                totalInserted += entities.Count;

                _logger.LogDebug(
                    "Lote insertado correctamente. Usuarios procesados: {TotalInserted}.",
                    totalInserted);
            }

            await _unitOfWork.CommitTransactionAsync(cts);

            _logger.LogInformation(
                "Sincronización completada. Total de usuarios sincronizados: {TotalUsers}.",
                totalInserted);

            return Result.Success<int, DomainError>(
                totalInserted);
        }
        catch (OperationCanceledException)
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw;
        }
        catch (NpgsqlException)
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            return Result.Failure<int, DomainError>(
                new DatabaseError.Unknown(
                    "Se perdió la conexión con PostgreSQL."));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            _logger.LogError(
                "Error inesperado durante la sincronización: {Message}",
                ex.Message);

            return Result.Failure<int, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }
    /// <summary>
    /// Obtener todos los usuarios de la base de datos local.
    /// </summary>
    public async Task<Result<List<UserModel>, DomainError>> GetAllUsersAsync()
    {
        _logger.LogInformation(
            "Obteniendo todos los usuarios de la base de datos local.");

        try
        {
            var response = await _repository.GetAllAsync();

            if (response.IsFailure)
            {
                _logger.LogError(
                    "No se pudieron obtener los usuarios de la base de datos.");

                return Result.Failure<List<UserModel>, DomainError>(
                    response.Error);
            }

            var models = response.Value
                .Select(u => u.ToModel())
                .ToList();

            _logger.LogInformation(
                "Se obtuvieron {TotalUsers} usuarios de la base de datos.",
                models.Count);

            return Result.Success<List<UserModel>, DomainError>(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al obtener todos los usuarios.");

            return Result.Failure<List<UserModel>, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    /// <summary>
    /// Obtener un usuario por ID: caché, base de datos y API.
    /// </summary>
    public async Task<Result<UserModel, DomainError>> GetUserByIdAsync(int id)
    {
        var key = $"user:{id}";

        // 1. Buscar en caché
        var cacheResult = await _cache.GetAsync(key);

        if (cacheResult.IsSuccess)
        {
            return cacheResult;
        }
        _logger.LogDebug(
            "Caché miss para el usuario {UserId}.", id);

        // 2. Buscar en base de datos
        var repositoryResult = await _repository.GetUserByIdAsync(id);

        if (repositoryResult.IsSuccess)
        {
            var user = repositoryResult.Value.ToModel();

            await _cache.SetAsync(key, user);

            _logger.LogInformation(
                "Usuario {UserId} obtenido de la base de datos y guardado en caché.",
                id);

            return Result.Success<UserModel, DomainError>(user);
        }

        // Solo consultar la API si el usuario no existe localmente.
        if (repositoryResult.Error is not DatabaseError.NotFound)
        {
            _logger.LogError(
                "Error al consultar el usuario {UserId} en la base de datos.",
                id);

            return Result.Failure<UserModel, DomainError>(
                repositoryResult.Error);
        }

        _logger.LogDebug(
            "Usuario {UserId} no encontrado localmente. Consultando la API.",
            id);

        // 3. Buscar en API
        var apiResult = await _api.GetByIdAsync(id);

        if (apiResult.IsFailure)
        {
            _logger.LogWarning(
                "No se pudo obtener el usuario {UserId} de la API.",
                id);

            return Result.Failure<UserModel, DomainError>(
                apiResult.Error);
        }

        var apiUser = apiResult.Value;

        await _cache.SetAsync(key, apiUser);

        _logger.LogInformation(
            "Usuario {UserId} obtenido de la API y guardado en caché.",
            id);

        return Result.Success<UserModel, DomainError>(apiUser);
    }

    /// <summary>
    /// Actualizar un usuario en la API, base de datos y caché.
    /// </summary>
    public async Task<Result<UserModel, DomainError>> UpdateUserAsync(
        int id,
        UpdateUserRequest request)
    {
        
        _logger.LogInformation(
            "Iniciando actualización del usuario {UserId}.", id);
        /*
        // 1. Actualizar en API
        var response = await _api.UpdateAsync(id, request);

        if (response.IsFailure)
        {
            _logger.LogWarning(
                "La API rechazó o no pudo completar la actualización del usuario {UserId}.",
                id);

            return Result.Failure<UserModel, DomainError>(
                response.Error);
        }

        var user = response.Value;

        _logger.LogInformation(
            "Usuario {UserId} actualizado en la API.",
            id);
        */
        // 2. Actualizar base de datos
        var responseDatabase = await _repository.UpdateAsync(
            request.ToModel().ToEntity(),
            id);

        if (responseDatabase.IsFailure)
        {
            _logger.LogError(
                "El usuario {UserId} se actualizó en la API, pero falló la actualización local.",
                id);

            return Result.Failure<UserModel, DomainError>(
                responseDatabase.Error);
        }

        _logger.LogInformation(
            "Usuario {UserId} actualizado en la base de datos.",
            id);

        // 3. Actualizar caché
        var responseCache = await _cache.SetAsync(
            $"user:{id}",
            request.ToModel());

        if (responseCache.IsFailure)
        {
            _logger.LogWarning(
                "El usuario {UserId} se actualizó en API y base de datos, pero falló la caché.",
                id);
        }
        else
        {
            _logger.LogDebug(
                "Caché actualizada para el usuario {UserId}.", id);
        }

        _logger.LogInformation(
            "Proceso de actualización del usuario {UserId} finalizado.",
            id);

        return Result.Success<UserModel, DomainError>(request.ToModel());
    }

    /// <summary>
    /// Eliminar un usuario de la API, base de datos y caché.
    /// </summary>
    public async Task<Result<bool, DomainError>> DeleteUserAsync(int id)
    {
        var key = $"user:{id}";

        _logger.LogInformation(
            "Iniciando eliminación del usuario {UserId}.", id);
        /*
        // 1. Eliminar de la API
        var response = await _api.DeleteAsync(id);

        if (response.IsFailure)
        {
            _logger.LogWarning(
                "No se pudo eliminar el usuario {UserId} de la API.",
                id);

            return Result.Failure<bool, DomainError>(
                response.Error);
        }

        _logger.LogInformation(
            "Usuario {UserId} eliminado de la API.", id);
            */

        // 2. Eliminar de base de datos
        var responseDatabase = await _repository.DeleteAsync(id);

        if (responseDatabase.IsFailure)
        {
            _logger.LogError(
                "El usuario {UserId} se eliminó de la API, pero falló la eliminación local.",
                id);

            return Result.Failure<bool, DomainError>(
                responseDatabase.Error);
        }

        _logger.LogInformation(
            "Usuario {UserId} eliminado de la base de datos.", id);

        // 3. Eliminar de caché
        var responseCache = await _cache.RemoveAsync(key);

        if (responseCache.IsFailure)
        {
            _logger.LogWarning(
                "El usuario {UserId} se eliminó de API y base de datos, pero falló la eliminación de caché.",
                id);
        }
        else
        {
            _logger.LogDebug(
                "Caché eliminada para el usuario {UserId}.", id);
        }

        _logger.LogInformation(
            "Proceso de eliminación del usuario {UserId} finalizado.",
            id);

        return Result.Success<bool, DomainError>(true);
    }

    /// <summary>
/// Crear un usuario en la API, base de datos y caché.
/// </summary>
    public async Task<Result<UserModel, DomainError>> CreateUserAsync(
        CreateUserDto request,
        CancellationToken cts = default)
    {
        _logger.LogInformation(
            "Iniciando creación del usuario.");

        // 1. Crear en API
        var response = await _api.CreateAsync(request);

        if (response.IsFailure)
        {
            _logger.LogWarning(
                "La API rechazó o no pudo completar la creación del usuario.");

            return Result.Failure<UserModel, DomainError>(
                response.Error);
        }

        var user = response.Value;

        _logger.LogInformation(
            "Usuario {UserId} creado en la API.",
            user.id);

        // 2. Crear en base de datos
        var responseDatabase = await _repository.CreateAsync(
            user.ToEntity());

        if (responseDatabase.IsFailure)
        {
            _logger.LogError(
                "El usuario {UserId} se creó en la API, pero falló la creación local.",
                user.id);

            return Result.Failure<UserModel, DomainError>(
                responseDatabase.Error);
        }

        _logger.LogInformation(
            "Usuario {UserId} creado en la base de datos.",
            user.id);

        // 3. Actualizar caché
        var responseCache = await _cache.SetAsync(
            $"user:{user.id}",
            user);

        if (responseCache.IsFailure)
        {
            _logger.LogWarning(
                "El usuario {UserId} se creó en API y base de datos, pero falló la caché.",
                user.id);
        }
        else
        {
            _logger.LogDebug(
                "Caché creada para el usuario {UserId}.",
                user.id);
        }

        _logger.LogInformation(
            "Proceso de creación del usuario {UserId} finalizado.",
            user.id);

        return Result.Success<UserModel, DomainError>(user);
    }
}