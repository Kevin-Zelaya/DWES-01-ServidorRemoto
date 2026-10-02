using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Cache.Common;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Cache;

public class InMemoryCache : IPostService
{
    private readonly IMemoryCache _cache;
    private const string CacheKey = "UserListCacheKey";
    
    // Semáforo asíncrono para proteger el acceso concurrente al ser un Singleton compartido
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public InMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    // ==========================================
    // OBTENER TODOS LOS USUARIOS
    // ==========================================
    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        if (!_cache.TryGetValue(CacheKey, out List<UserModel> users))
        {
            await _semaphore.WaitAsync();
            try
            {
                // Doble comprobación segura tras adquirir el bloqueo
                if (!_cache.TryGetValue(CacheKey, out users))
                {
                    users = new List<UserModel>();
                    SetUsersInCache(users);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return users ?? new List<UserModel>();
    }

    // Método auxiliar privado para aplicar el tiempo de vida (TTL) predeterminado
    private void SetUsersInCache(List<UserModel> users)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };
        _cache.Set(CacheKey, users, cacheEntryOptions);
    }

    // ==========================================
    // CREAR USUARIO
    // ==========================================
    public async Task<Result<UserModel, DomainError>> CreateUser(CreateUserDto request)
    {
        await _semaphore.WaitAsync();
        try
        {
            var users = await GetAllUsersAsync();

            if (users.Any(u => u.email.Equals(request.email, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Failure<UserModel, DomainError>(
                    new CacheError.AlreadyExists("Usuario con email", request.email)
                );
            }

            int newId = users.Count > 0 ? users.Max(u => u.id) + 1 : 1;

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

            users.Add(newUser);
            SetUsersInCache(users);

            return Result.Success<UserModel, DomainError>(newUser);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // ==========================================
    // ACTUALIZAR USUARIO
    // ==========================================
    public async Task<Result<UserModel, DomainError>> UpdateUser(UpdateUserRequest request)
    {
        if (request.id <= 0)
        {
            return Result.Failure<UserModel, DomainError>(
                new CacheError.InvalidOperation("El ID proporcionado para actualizar en la caché no es válido.")
            );
        }

        await _semaphore.WaitAsync();
        try
        {
            var users = await GetAllUsersAsync();
            var existingUser = users.FirstOrDefault(u => u.id == request.id);

            if (existingUser == null)
            {
                return Result.Failure<UserModel, DomainError>(
                    new CacheError.NotFound("Usuario", request.id)
                );
            }

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

            SetUsersInCache(users);

            return Result.Success<UserModel, DomainError>(existingUser);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // ==========================================
    // ELIMINAR USUARIO
    // ==========================================
    public async Task<Result<bool, DomainError>> DeleteUser(int id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var users = await GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.id == id);

            if (user == null)
            {
                return Result.Failure<bool, DomainError>(
                    new CacheError.NotFound("Usuario", id)
                );
            }

            users.Remove(user);
            SetUsersInCache(users);

            return Result.Success<bool, DomainError>(true);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}