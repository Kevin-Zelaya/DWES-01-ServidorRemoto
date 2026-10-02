using CSharpFunctionalExtensions;
using RepositorioRemoto.Cache.Common;

public class UserService(
    IRepository _repository,
    IUserApiService _api,
    UnitOfWork _unitOfWork,
    ICache<UserModel> _cache
)
{
    /// <summary>
    /// Saga, sincronizar bases de datos
    /// </summary>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async Task<Result<bool, DomainError>>SyncUsersAsync(
        CancellationToken cts = default
    )
    {
        // 1. Obtener datos de la api 
        var apiResult = await _api.GetAllAsync(cts);

        if (apiResult.IsFailure)
        {
            return apiResult.ConvertFailure<bool>();
        }
        // 2. Iniciamos la transacción
        await _unitOfWork.BeginTransactionAsync(cts);

        try
        {
            // 3. Limpiamos la base de datos
            var deleteResult = await _repository.DeleteAllAsync();

            if (deleteResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(cts);
                return deleteResult.ConvertFailure<bool>();
            }

            // 4. Insertamos los datos de la api en la base de datos por lotes (batch)
            foreach (var batch in apiResult.Value.Chunk(AppConfig.BatchSettings.BatchSize))
            {
                var entities = batch.Select(u => u.ToEntity()).ToList();
                var createResult = await _repository.CreateRangeAsync(entities);

                if (createResult.IsFailure)
                {
                    await _unitOfWork.RollbackTransactionAsync(cts);
                    return createResult.ConvertFailure<bool>();
                }
            }
            // En el futuro se trabaja con cach

            // 5. Confirmamos la transacción
            await _unitOfWork.CommitTransactionAsync(cts);

            return Result.Success<bool, DomainError>(true);
        } catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cts);
            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message)
            );
        }
    }
    /// <summary>
    /// Obtener todos los datos de la base de datos
    /// </summary>
    /// <returns></returns>
    public async Task<Result<List<UserModel>, DomainError>> GetAllUsersAsync()
    {
        // 1. Obtener datos de la base de datos

        var response = await _repository.GetAllAsync();
        var models = response.Value
            .Select(u => u.ToModel())
            .ToList();
        return Result.Success<List<UserModel>, DomainError>(models);
    }
    
    /// <summary>
    /// SAGA, Obtener usuario por id
    /// 1. Consulta en la cache
    /// 2. Consulta en la db local
    /// 3. Consulta en la api externa
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result<UserModel, DomainError>> GetUserByIdAsync(int id)
    {
        var key = $"user:{id}";

        // 1. Buscar en caché
        var cacheResult = await _cache.GetAsync(key);

        if (cacheResult.IsSuccess)
            return cacheResult;

        // Si falla la caché, continuamos con el repositorio.

        // 2. Buscar en base de datos
        var repositoryResult = await _repository.GetUserByIdAsync(id);

        if (repositoryResult.IsSuccess)
        {
            var user = repositoryResult.Value.ToModel();

            await _cache.SetAsync(key, user);

            return Result.Success<UserModel, DomainError>(user);
        }

        // Solo continuamos si el usuario no existe en la base de datos.
        if (repositoryResult.Error is not DatabaseError.NotFound)
            return Result.Failure<UserModel, DomainError>(
                repositoryResult.Error);

        // 3. Buscar en la API
        var apiResult = await _api.GetByIdAsync(id);

        if (apiResult.IsFailure)
            return Result.Failure<UserModel, DomainError>(apiResult.Error);

        var apiUser = apiResult.Value;

        await _cache.SetAsync(key, apiUser);

        return Result.Success<UserModel, DomainError>(apiUser);
    }
    /// <summary>
    /// Actualizar usuario
    /// 1. Actualizar en la api
    /// 2. Actualizar en la base de datos
    /// 3. Actualizar en la caché
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Result<UserModel, DomainError>> UpdateUserAsync(
        int id,
        UpdateUserRequest request)
    {
        // 1. Actualizar en la API (fuente oficial)
        var response = await _api.UpdateAsync(id, request);

        if (response.IsFailure)
            return Result.Failure<UserModel, DomainError>(
                response.Error);

        var user = response.Value;

        // 2. Actualizar la base de datos local
        var responseDatabase = await _repository.UpdateAsync(
            user.ToEntity(),
            id);

        if (responseDatabase.IsFailure)
        {
            // La API ya se ha actualizado.
            // Registrar el error para sincronizar la BD posteriormente.
            return Result.Failure<UserModel, DomainError>(
                responseDatabase.Error);
        }

        // 3. Actualizar la caché
        var responseCache = await _cache.SetAsync(
            $"user:{id}",
            user);

        // La caché es auxiliar: si falla, el usuario ya está actualizado
        // en la API y en la BD.
        return Result.Success<UserModel, DomainError>(user);
    }

    /// <summary>
    /// Eliminar usuario
    /// 1. Eliminar en la api
    /// 2. Eliminar en la base de datos
    /// 3. Eliminar en la caché
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Result<bool, DomainError>> DeleteUserAsync(int id)
    {
        var key = $"user:{id}";

        // 1. Eliminar usuario de la API
        var response = await _api.DeleteAsync(id);

        if (response.IsFailure)
            return Result.Failure<bool, DomainError>(
                response.Error);

        // 2. Eliminar de la base de datos
        var responseDatabase = await _repository.DeleteAsync(id);

        if (responseDatabase.IsFailure)
        {
            // La API ya ha eliminado el usuario.
            // Registrar el error para sincronizar la BD posteriormente.
            return Result.Failure<bool, DomainError>(
                responseDatabase.Error);
        }

        // 3. Eliminar de la caché
        var responseCache = await _cache.RemoveAsync(key);

        // La caché es auxiliar: si falla, la eliminación
        // en la API y en la BD ya se ha realizado.
        return Result.Success<bool, DomainError>(true);
    }


}