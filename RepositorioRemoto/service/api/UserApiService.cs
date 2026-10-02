using CSharpFunctionalExtensions;
using Refit;

public class UserApiService(
    IUserApi _api
) : IUserApiService, Scoped
{

    public void Typeof()
    {
        string miNamespace = this.GetType().Namespace;
        Console.WriteLine($"Namespace: {miNamespace}");
    }    
    public async Task<Result<List<UserModel>, DomainError>> GetAllAsync(CancellationToken cts = default)
    {
        try
        {
            var result = await _api.GetUsuariosAsync(cts);
            return Result.Success<List<UserModel>, DomainError>(result.Select(u => u.ToModel()).ToList());
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<List<UserModel>, DomainError>(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex)
        {
            return Result.Failure<List<UserModel>, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<List<UserModel>, DomainError>(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }

    /// <summary>
    /// Obtener usuario por id, cancelation token "default" por defecto
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async Task<Result<UserModel, DomainError>> GetByIdAsync(int id, CancellationToken cts = default)
    {
        try
        {
            var usuario = await _api.GetUsuarioByIdAsync(id);
            return usuario is not null
                ? Result.Success<UserModel, DomainError>(usuario.ToModel())
                : Result.Failure<UserModel, DomainError>(
                    new ApiError.NotFound("Usuario", id)
                );
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.NotFound("User", id)
            );
        }
        catch (ApiException ex)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }

    /// <summary>
    /// Crear usuario, cancelation token "default" por defecto
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async Task<Result<UserModel, DomainError>> CreateAsync(CreateUserDto request, CancellationToken cts = default)
    {
        try
        {
            var creado = await _api.CreateUsuarioAsync(request, cts);
            return Result.Success<UserModel, DomainError>(creado.ToModel());
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.BadRequest()
            );
        }
        catch (ApiException ex)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }
    
    /// <summary>
    /// Actualizar usuario, cancelation token "default" por defecto
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async Task<Result<UserModel, DomainError>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cts = default)
    {
        try
        {
            var actualizado = await _api.UpdateUsuarioAsync(id, request);
            return Result.Success<UserModel, DomainError>(actualizado.ToModel());
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.BadRequest()
            );
        }
        catch (ApiException ex)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<UserModel, DomainError>(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }
    /// <summary>
    /// Eliminar, cancelation token "default" por defecto
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async Task<Result<bool, DomainError>> DeleteAsync(int id, CancellationToken cts = default)
    {
        try
        {
            await _api.DeleteUsuarioAsync(id, cts);
            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<bool, DomainError>(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex)
        {
            return Result.Failure<bool, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result.Failure<bool, DomainError>(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }    
 

}