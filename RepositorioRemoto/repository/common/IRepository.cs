using CSharpFunctionalExtensions;

public interface IRepository
{
    public Task<Result<List<UserEntity>, DomainError>> GetAllAsync();
    public Task<Result<UserEntity, DomainError>> CreateAsync(UserEntity user);
    public Task<Result<UserEntity, DomainError>> CreateRangeAsync(IEnumerable<UserEntity> users);
    public Task<Result<UserEntity, DomainError>> UpdateAsync(UserEntity user, int id);
    public Task<Result<bool, DomainError>> DeleteAsync(int userId);
    public Task<Result<bool, DomainError>> DeleteAllAsync(CancellationToken cts = default);
    public Task<Result<UserEntity, DomainError>> GetUserByIdAsync(int id, CancellationToken cts = default);
    
}