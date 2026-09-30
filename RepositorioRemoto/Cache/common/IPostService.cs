using System.Collections.Generic;
using System.Threading.Tasks;


namespace RepositorioRemoto.Cache.Common;

public interface IPostService
{
    Task<List<UserModel>> GetAllUsersAsync();
    Task<Result<UserModel, DomainError>> CreateUser(UserModel user);
    Task<Result<UserModel, DomainError>> UpdateUser(UserModel user);
    Task<Result<bool, DomainError>> DeleteUser(int id);
}