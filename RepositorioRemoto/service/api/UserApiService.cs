using System.Net;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Refit;

public class UserApiService(
    IUserApi api,
    ILogger<UserApiService> logger
) : IUserApiService, Scoped
{
    public void Typeof()
    {
        logger.LogDebug(
            "Namespace del servicio: {Namespace}",
            GetType().Namespace);
    }

    /// <summary>
    /// Obtener todos los usuarios.
    /// </summary>
    public async Task<Result<List<UserModel>, DomainError>> GetAllAsync(
        CancellationToken cts = default)
    {
        logger.LogInformation("Obteniendo todos los usuarios.");

        try
        {
            var usuarios = await api.GetUsuariosAsync(cts);
            var modelos = usuarios.Select(u => u.ToModel()).ToList();

            logger.LogInformation(
                "Se obtuvieron {TotalUsuarios} usuarios.",
                modelos.Count);

            return Result.Success<List<UserModel>, DomainError>(modelos);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                ex,
                "La obtención de todos los usuarios fue cancelada.");

            return Result.Failure<List<UserModel>, DomainError>(
                new ApiError.Timeout());
        }
        catch (ApiException ex)
        {
            logger.LogError(
                ex,
                "Error HTTP al obtener todos los usuarios. Código: {StatusCode}",
                ex.StatusCode);

            return Result.Failure<List<UserModel>, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Error de conexión al obtener todos los usuarios.");

            return Result.Failure<List<UserModel>, DomainError>(
                new ApiError.NetworkError(ex.Message));
        }
    }

    /// <summary>
    /// Obtener un usuario por su ID.
    /// </summary>
    public async Task<Result<UserModel, DomainError>> GetByIdAsync(
        int id,
        CancellationToken cts = default)
    {
        logger.LogInformation(
            "Obteniendo usuario con ID {UserId}.", id);

        try
        {
            var usuario = await api.GetUsuarioByIdAsync(id, cts);

            if (usuario is null)
            {
                logger.LogWarning(
                    "No se encontró el usuario con ID {UserId}.", id);

                return Result.Failure<UserModel, DomainError>(
                    new ApiError.NotFound("Usuario", id));
            }

            logger.LogInformation(
                "Usuario con ID {UserId} obtenido correctamente.", id);

            return Result.Success<UserModel, DomainError>(
                usuario.ToModel());
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                ex,
                "La consulta del usuario {UserId} fue cancelada.", id);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.Timeout());
        }
        catch (ApiException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning(
                ex,
                "La API no encontró el usuario con ID {UserId}.", id);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.NotFound("Usuario", id));
        }
        catch (ApiException ex)
        {
            logger.LogError(
                ex,
                "Error HTTP al obtener el usuario {UserId}. Código: {StatusCode}",
                id,
                ex.StatusCode);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Error de conexión al obtener el usuario {UserId}.", id);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.NetworkError(ex.Message));
        }
    }

    /// <summary>
    /// Crear un usuario.
    /// </summary>
    public async Task<Result<UserModel, DomainError>> CreateAsync(
        CreateUserDto request,
        CancellationToken cts = default)
    {
        logger.LogInformation("Creando un usuario.");

        try
        {
            var creado = await api.CreateUsuarioAsync(request, cts);
            var modelo = creado.ToModel();

            logger.LogInformation(
                "Usuario creado correctamente.");

            return Result.Success<UserModel, DomainError>(modelo);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                ex,
                "La creación del usuario fue cancelada.");

            return Result.Failure<UserModel, DomainError>(
                new ApiError.Timeout());
        }
        catch (ApiException ex) when (
            ex.StatusCode == HttpStatusCode.BadRequest)
        {
            logger.LogWarning(
                ex,
                "La API rechazó los datos enviados para crear el usuario.");

            return Result.Failure<UserModel, DomainError>(
                new ApiError.BadRequest());
        }
        catch (ApiException ex)
        {
            logger.LogError(
                ex,
                "Error HTTP al crear el usuario. Código: {StatusCode}",
                ex.StatusCode);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Error de conexión al crear el usuario.");

            return Result.Failure<UserModel, DomainError>(
                new ApiError.NetworkError(ex.Message));
        }
    }

    /// <summary>
    /// Actualizar un usuario.
    /// </summary>
    public async Task<Result<UserModel, DomainError>> UpdateAsync(
        int id,
        UpdateUserRequest request,
        CancellationToken cts = default)
    {
        logger.LogInformation(
            "Actualizando usuario con ID {UserId}.", id);

        try
        {
            var actualizado =
                await api.UpdateUsuarioAsync(id, request, cts);

            var modelo = actualizado.ToModel();

            logger.LogInformation(
                "Usuario con ID {UserId} actualizado correctamente.", id);

            return Result.Success<UserModel, DomainError>(modelo);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                ex,
                "La actualización del usuario {UserId} fue cancelada.", id);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.Timeout());
        }
        catch (ApiException ex) when (
            ex.StatusCode == HttpStatusCode.BadRequest)
        {
            logger.LogWarning(
                ex,
                "La API rechazó la actualización del usuario {UserId}.", id);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.BadRequest());
        }
        catch (ApiException ex)
        {
            logger.LogError(
                ex,
                "Error HTTP al actualizar el usuario {UserId}. Código: {StatusCode}",
                id,
                ex.StatusCode);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Error de conexión al actualizar el usuario {UserId}.", id);

            return Result.Failure<UserModel, DomainError>(
                new ApiError.NetworkError(ex.Message));
        }
    }

    /// <summary>
    /// Eliminar un usuario.
    /// </summary>
    public async Task<Result<bool, DomainError>> DeleteAsync(
        int id,
        CancellationToken cts = default)
    {
        logger.LogInformation(
            "Eliminando usuario con ID {UserId}.", id);

        try
        {
            await api.DeleteUsuarioAsync(id, cts);

            logger.LogInformation(
                "Usuario con ID {UserId} eliminado correctamente.", id);

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                ex,
                "La eliminación del usuario {UserId} fue cancelada.", id);

            return Result.Failure<bool, DomainError>(
                new ApiError.Timeout());
        }
        catch (ApiException ex) when (
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogWarning(
                ex,
                "No se encontró el usuario {UserId} para eliminar.", id);

            return Result.Failure<bool, DomainError>(
                new ApiError.NotFound("Usuario", id));
        }
        catch (ApiException ex)
        {
            logger.LogError(
                ex,
                "Error HTTP al eliminar el usuario {UserId}. Código: {StatusCode}",
                id,
                ex.StatusCode);

            return Result.Failure<bool, DomainError>(
                new ApiError.HttpFailure(ex.StatusCode, ex.Message));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Error de conexión al eliminar el usuario {UserId}.", id);

            return Result.Failure<bool, DomainError>(
                new ApiError.NetworkError(ex.Message));
        }
    }
}