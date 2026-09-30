using Cliente_Http_con_refit.Dto;
using Refit;

public class UserApiService(
    IUserApi _api
) : IUserApiService
{
    
    public async Task<Result<List<UserDto>, DomainError>> GetAllAsync(CancellationToken cts = default)
    {
        try
        {
            var result = await _api.GetUsuariosAsync(cts);
            return Result<List<UserDto>, DomainError>.ok(result);
        }
        catch (OperationCanceledException)
        {
            return Result<List<UserDto>, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex)
        {
            return Result<List<UserDto>, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<List<UserDto>, DomainError>.Fail(
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
    public async Task<Result<UserDto, DomainError>> GetByIdAsync(int id, CancellationToken cts = default)
    {
        try
        {
            var usuario = await _api.GetUsuarioByIdAsync(id);
            return usuario is not null
                ? Result<UserDto, DomainError>.ok(usuario)
                : Result<UserDto, DomainError>.Fail(
                    new ApiError.NotFound("Usuario", id)
                );
        }
        catch (OperationCanceledException)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.NotFound("User", id)
            );
        }
        catch (ApiException ex)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<UserDto, DomainError>.Fail(
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
    public async Task<Result<UserDto, DomainError>> CreateAsync(CreateUserDto request, CancellationToken cts = default)
    {
        try
        {
            var creado = await _api.CreateUsuarioAsync(request, cts);
            return Result<UserDto, DomainError>.ok(creado);
        }
        catch (OperationCanceledException)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.BadRequest()
            );
        }
        catch (ApiException ex)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<UserDto, DomainError>.Fail(
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
    public async Task<Result<UserDto, DomainError>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cts = default)
    {
        try
        {
            var actualizado = await _api.UpdateUsuarioAsync(id, request);
            return Result<UserDto, DomainError>.ok(actualizado);
        }
        catch (OperationCanceledException)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.BadRequest()
            );
        }
        catch (ApiException ex)
        {
            return Result<UserDto, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<UserDto, DomainError>.Fail(
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
            return Result<bool, DomainError>.ok(true);
        }
        catch (OperationCanceledException)
        {
            return Result<bool, DomainError>.Fail(
                new ApiError.Timeout()
            );
        }
        catch (ApiException ex)
        {
            return Result<bool, DomainError>.Fail(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message)
            );
        }
        catch (HttpRequestException ex)
        {
            return Result<bool, DomainError>.Fail(
                new ApiError.NetworkError(ex.Message)
            );
        }
    }    

}