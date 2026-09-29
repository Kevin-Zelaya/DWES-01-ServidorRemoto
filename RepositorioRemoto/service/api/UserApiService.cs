using Refit;

public class UserService(
    IUserApi _api
)
{
    
    public async Task<Result<List<UserModel>, DomainError>> GetAllAsync(CancellationToken cts = default)
    {
        try
        {
            var result = await _api.GetUsuariosAsync(cts);
            return Result<List<UserModel>, DomainError>.ok(result);
        }
        catch (OperationCanceledException)
        {
            return Result<List<UserModel>, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex)
        {
            return Result<List<UserModel>, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<List<UserModel>, DomainError>.Fail(
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
                ? Result<UserModel, DomainError>.ok(usuario)
                : Result<UserModel, DomainError>.Fail(
                    new ApiError.NotFound("Usuario", id)
                );
        }
        catch (OperationCanceledException)
        {
            return Result<UserModel, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<UserModel, DomainError>.Fail(
                new ApiError.NotFound("User", id)
            );
        }
        catch (ApiException ex)
        {
            return Result<UserModel, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<UserModel, DomainError>.Fail(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }

}