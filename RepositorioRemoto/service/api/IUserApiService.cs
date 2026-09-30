using Cliente_Http_con_refit.Dto;

public interface IUserApiService
{
    /// <summary>
    /// Obtener todos los usuarios
    /// </summary>
    /// <param name="cts"></param>
    /// <returns></returns>
    public Task<Result<List<UserDto>, DomainError>> GetAllAsync(CancellationToken cts = default);
    /// <summary>
    /// Obtener usuario especifico
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public Task<Result<UserDto, DomainError>> GetByIdAsync(int id, CancellationToken cts = default);

    public Task<Result<UserDto, DomainError>> CreateAsync(CreateUserDto request, CancellationToken cts = default);

    public Task<Result<UserDto, DomainError>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cts = default);

    public Task<Result<bool, DomainError>> DeleteAsync(int id, CancellationToken cts = default);


}
