using Cliente_Http_con_refit.Dto;
using CSharpFunctionalExtensions;

public interface IUserApiService
{
    /// <summary>
    /// Obtener todos los usuarios
    /// </summary>
    /// <param name="cts"></param>
    /// <returns></returns>
    public Task<Result<List<UserModel>, DomainError>> GetAllAsync(CancellationToken cts = default);
    /// <summary>
    /// Obtener usuario especifico
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public Task<Result<UserModel, DomainError>> GetByIdAsync(int id, CancellationToken cts = default);

    public Task<Result<UserModel, DomainError>> CreateAsync(CreateUserDto request, CancellationToken cts = default);

    public Task<Result<UserModel, DomainError>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cts = default);

    public Task<Result<bool, DomainError>> DeleteAsync(int id, CancellationToken cts = default);


}
