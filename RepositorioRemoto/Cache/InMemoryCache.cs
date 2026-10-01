using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Cliente_Http_con_refit.Dto;
using RepositorioRemoto.Cache.Common;

namespace RepositorioRemoto.Cache;

public class InMemoryCache : IPostService
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "UserListCacheKey";

    // Inyectamos el servicio IMemoryCache de .NET
    public InMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    // ==========================================
    // OBTENER TODOS LOS USUARIOS
    // ==========================================
    public async Task<List<UserModel>> GetAllUsersAsync() // falta result
    {
        // Comprobamos si la lista ya existe en memoria; si no, la inicializamos vacía
        if (!_cache.TryGetValue(CacheKey, out List<UserModel> users))
        {
            users = new List<UserModel>();
            _cache.Set(CacheKey, users);
        }

        return users;
    }

    // ==========================================
    //  CREAR USUARIO
    // ==========================================
    public async Task<Result<UserModel, DomainError>> CreateUser(CreateUserDto request)
    {
        var users = await GetAllUsersAsync();

        // Generamos un ID autoincremental provisional para la memoria
        int newId = users.Count > 0 ? users.Max(u => u.id) + 1 : 1;

        // Mapeo manual desde CreateUserDto hacia UserModel
        var newUser = new UserModel
        {
            id = newId,
            name = request.name,
            username = request.username,
            email = request.email,
            phone = request.phone,
            website = request.website,
            address = request.address != null ? new Address
            {
                street = request.address.street,
                suite = request.address.suite,
                city = request.address.city,
                zipcode = request.address.zipcode,
                geo = request.address.geo != null ? new Geo { lat = request.address.geo.lat, lng = request.address.geo.lng } : null
            } : null,
            company = request.company != null ? new Company
            {
                name = request.company.name,
                catchPhrase = request.company.catchPhrase,
                bs = request.company.bs
            } : null
        };

        // Añadimos el usuario a la lista en caché y actualizamos el almacenamiento
        users.Add(newUser);
        _cache.Set(CacheKey, users);

        // Devolvemos el resultado exitoso utilizando .ok()
        return Result<UserModel, DomainError>.ok(newUser);
    }

    // ==========================================
    // ACTUALIZAR USUARIO
    // ==========================================
    public async Task<Result<UserModel, DomainError>> UpdateUser(UpdateUserRequest request)
    {
        var users = await GetAllUsersAsync();
        var existingUser = users.FirstOrDefault(u => u.id == request.id);

        // Si el usuario no existe en la caché, devolvemos un error de dominio controlado
        if (existingUser == null)
        {
            return Result<UserModel, DomainError>.Fail(
                new DomainError("El usuario no existe en la caché.", System.Net.HttpStatusCode.NotFound)
            );
        }

        // Actualizamos las propiedades del usuario encontrado con los datos del request
        existingUser.name = request.name;
        existingUser.username = request.username;
        existingUser.email = request.email;
        existingUser.phone = request.phone;
        existingUser.website = request.website;
        
        if (request.address != null)
        {
            existingUser.address = new Address
            {
                street = request.address.street,
                suite = request.address.suite,
                city = request.address.city,
                zipcode = request.address.zipcode,
                geo = request.address.geo != null ? new Geo { lat = request.address.geo.lat, lng = request.address.geo.lng } : null
            };
        }

        if (request.company != null)
        {
            existingUser.company = new Company
            {
                name = request.company.name,
                catchPhrase = request.company.catchPhrase,
                bs = request.company.bs
            };
        }

        // Guardamos los cambios en la caché
        _cache.Set(CacheKey, users);

        return Result<UserModel, DomainError>.ok(existingUser);
    }

    // ==========================================
    // ELIMINAR USUARIO
    // ==========================================
    public async Task<Result<bool, DomainError>> DeleteUser(int id)
    {
        var users = await GetAllUsersAsync();
        var user = users.FirstOrDefault(u => u.id == id);

        // Si no se encuentra el usuario, devolvemos error
        if (user == null)
        {
            return Result<bool, DomainError>.Fail(
                new DomainError("El usuario a eliminar no existe.", System.Net.HttpStatusCode.NotFound)
            );
        }

        // Removemos de la lista y actualizamos la caché
        users.Remove(user);
        _cache.Set(CacheKey, users);

        return Result<bool, DomainError>.ok(true);
    }
}