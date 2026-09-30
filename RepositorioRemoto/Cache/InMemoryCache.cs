using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using RepositorioRemoto.Cache.Common;


namespace RepositorioRemoto.Cache;

public class InMemoryCache : IPostService
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "UserListCacheKey";

    public InMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        if (!_cache.TryGetValue(CacheKey, out List<UserModel> users))
        {
            users = new List<UserModel>();
            _cache.Set(CacheKey, users);
        }

        return users;
    }

    public async Task<Result<UserModel, DomainError>> CreateUser(UserModel user)
    {
        var users = await GetAllUsersAsync();

        user.id = users.Count > 0 ? users.Max(u => u.id) + 1 : 1;

        users.Add(user);
        _cache.Set(CacheKey, users);

        // Usamos .ok() en lugar de Success
        return Result<UserModel, DomainError>.ok(user);
    }

    public async Task<Result<UserModel, DomainError>> UpdateUser(UserModel user)
    {
        var users = await GetAllUsersAsync();
        var existingUser = users.FirstOrDefault(u => u.id == user.id);

        if (existingUser == null)
        {
            // Usamos .Fail() en lugar de Failure
            return Result<UserModel, DomainError>.Fail(
                new DomainError("El usuario no existe en la caché.", System.Net.HttpStatusCode.NotFound)
            );
        }

        existingUser.name = user.name;
        existingUser.email = user.email;
        existingUser.username = user.username;
        existingUser.address = user.address;
        existingUser.phone = user.phone;
        existingUser.website = user.website;
        existingUser.company = user.company;

        _cache.Set(CacheKey, users);

        return Result<UserModel, DomainError>.ok(existingUser);
    }

    public async Task<Result<bool, DomainError>> DeleteUser(int id)
    {
        var users = await GetAllUsersAsync();
        var user = users.FirstOrDefault(u => u.id == id);

        if (user == null)
        {
            return Result<bool, DomainError>.Fail(
                new DomainError("El usuario a eliminar no existe.", System.Net.HttpStatusCode.NotFound)
            );
        }

        users.Remove(user);
        _cache.Set(CacheKey, users);

        return Result<bool, DomainError>.ok(true);
    }
}