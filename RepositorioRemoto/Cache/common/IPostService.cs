using System.Collections.Generic;
using System.Threading.Tasks;
using Cliente_Http_con_refit.Dto;
namespace RepositorioRemoto.Cache.Common;

public interface IPostService
{
    // Obtiene la lista completa de usuarios almacenados en caché
    Task<List<UserModel>> GetAllUsersAsync();

    // Crea un nuevo usuario a partir de un DTO de creación
    Task<Result<UserModel, DomainError>> CreateUser(CreateUserDto request);

    // Actualiza un usuario existente usando un DTO de petición
    Task<Result<UserModel, DomainError>> UpdateUser(UpdateUserRequest request);

    // Elimina un usuario por su identificador único
    Task<Result<bool, DomainError>> DeleteUser(int id);
}