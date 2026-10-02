using CSharpFunctionalExtensions;

public class UserService(
    IRepository _repository,
    IUserApiService _api,
    UnitOfWork _unitOfWork
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
        // 1. Buscar en la caché

        // 2. Buscar en la base de datos
        var response = await _repository.GetUserByIdAsync(id);
        if (response.IsFailure)
        {
            // 3. Buscamos en la api
            var responseApi = await _api.GetByIdAsync(id);
            if (responseApi.IsFailure)
            {
                // 4. Por último si no se encuentra en la api retornamos error
                return Result.Failure<UserModel, DomainError>(responseApi.Error);
            }
            return Result.Success<UserModel, DomainError>(response.Value.ToModel());
        }
        return Result.Success<UserModel, DomainError>(response.Value.ToModel());
    }

    public async Task<Result<UserModel, DomainError>> UpdateUserAsync(
        int id,
        UpdateUserRequest request
    )
    {
        // 1. Actualizar en la api
        var response = await _api.UpdateAsync(id, request);
        if (response.IsSuccess)
        {
            // 2. Actualizamos en la bd
            var bdResponse = await _repository.UpdateAsync(response.Value.ToEntity(), id);
            
            return Result.Success<UserModel, DomainError>(
                bdResponse.Value.ToModel()
            );
        } 
        return Result.Failure<UserModel, DomainError>(
            response.Error 
        );
    }

}