using CSharpFunctionalExtensions;

public interface IUserService
{
    public Task<Result<int, DomainError>> SyncUsersAsync(CancellationToken cts = default);
    public Task<Result<List<UserModel>, DomainError>> GetAllUsersAsync();
    public Task<Result<UserModel, DomainError>> GetUserByIdAsync(int id);
    public Task<Result<UserModel, DomainError>> UpdateUserAsync(int id,
        UpdateUserRequest request);
    public Task<Result<bool, DomainError>> DeleteUserAsync(int id);
    public Task<Result<UserModel, DomainError>> CreateUserAsync(CreateUserDto dto, CancellationToken cts = default);
}