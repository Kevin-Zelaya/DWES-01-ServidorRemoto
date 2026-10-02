using Refit;

[Headers("content-type: application/json")]
public interface IUserApi
{
    /// <summary>
    /// Obtener todos los usuarios
    /// </summary>
    [Get("/users")]
    Task<List<UserDto>> GetUsuariosAsync(CancellationToken cts = default);

    /// <summary>
    /// Obtener usuario por id
    /// </summary>
    [Get("/users/{id}")]
    Task<UserDto?> GetUsuarioByIdAsync(int id, CancellationToken cts = default);

    /// <summary>
    /// Crear usuario 
    /// </summary>
    [Post("/users")]
    Task<UserDto> CreateUsuarioAsync([Body] CreateUserDto request, CancellationToken cts = default);

    /// <summary>
    /// Actualizar usuario
    /// </summary>
    [Put("/users/{id}")]
    Task<UserDto> UpdateUsuarioAsync(int id, [Body] UpdateUserRequest request, CancellationToken cts = default);

    /// <summary>
    /// Eliminar usuario
    /// </summary>
    [Delete("/users/{id}")]
    Task<bool> DeleteUsuarioAsync(int id, CancellationToken cts = default);
}
