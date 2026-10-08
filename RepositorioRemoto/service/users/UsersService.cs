using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using RepositorioRemoto.Cache.Common;

public class UserService(
    IRepository _repository,
    IUserApiService _api,
    IUnitOfWork _unitOfWork,
    ICache<UserModel> _cache,
    ILogger<UserService> _logger,
    INotificationService _notification
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
        // Obtenemos los usuarios desde la api
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

        try
        {
            // Iniciar transacción 
            await _unitOfWork.BeginTransactionAsync(cts);
            // Vaciar base de datos
            await _repository.DeleteAllAsync();
            // Limpiar cache
            await _cache.ClearAsync();
            // Mapear a dto
            // var users = apiResult.Value
            //     .Select(u => u.ToDto())
            //     .ToList();

            var totalInserted = 0;
            // Iterar en grupos (El tamaño de los batch se especifica en el appsettings)
            foreach (var batch in apiResult.Value.Chunk(
                AppConfig.BatchSettings.BatchSize))
            {
                cts.ThrowIfCancellationRequested();

                var entities = batch
                    .Select(u => u.ToEntity())
                    .ToList();

                var result =
                    await _repository.CreateRangeAsync(entities);
                // Si alguno falla, rollback
                if (result.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(
                        CancellationToken.None);

                    return Result.Failure<int, DomainError>(
                        result.Error);
                }
                totalInserted += entities.Count;
            }
            // Confirmar transacción
            await _unitOfWork.CommitTransactionAsync(cts);

            return Result.Success<int, DomainError>(
                totalInserted);
        }
        catch (OperationCanceledException)
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            _logger.LogError(
                ex,
                "Error durante la sincronización.");

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
        //Notificar actualización
        _notification.NotifyUserUpdated(responseDatabase.Value.ToModel());
        return Result.Success<UserModel, DomainError>(request.ToModel());
    }

    /// <summary>
    /// Eliminar un usuario de la API, base de datos y caché.
    /// </summary>
    public async Task<Result<bool, DomainError>> DeleteUserAsync(int id)
    {
        var key = $"user:{id}";
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
        // Notificar eliminacion
        _notification.NotifyUserDeleted(id);
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
        // Notificar
        _notification.NotifyUserCreated(user);

        return Result.Success<UserModel, DomainError>(user);
    }
}