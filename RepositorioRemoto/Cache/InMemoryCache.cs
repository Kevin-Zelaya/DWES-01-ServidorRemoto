using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Cache.Common;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Cache;

using Microsoft.Extensions.Caching.Memory;

public class InMemoryCache<T> : ICache<T>
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    // ==========================================
    // OBTENER UN ELEMENTO
    // ==========================================
    public Task<Result<T, DomainError>> GetAsync(string key)
    {
        if (_memoryCache.TryGetValue(key, out T? value)
            && value is not null)
        {
            return Task.FromResult(
                Result.Success<T, DomainError>(value));
        }

        return Task.FromResult(
            Result.Failure<T, DomainError>(
                new CacheError.NotFound(key)));
    }

    // ==========================================
    // GUARDAR UN ELEMENTO
    // ==========================================
    public Task<Result<bool, DomainError>> SetAsync(
        string key,
        T value,
        TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                expiration ?? TimeSpan.FromMinutes(30)
        };

        _memoryCache.Set(key, value, options);

        return Task.FromResult(
            Result.Success<bool, DomainError>(true));
    }

    // ==========================================
    // ELIMINAR UN ELEMENTO
    // ==========================================
    public Task<Result<bool, DomainError>> RemoveAsync(string key)
    {
        bool exists = _memoryCache.TryGetValue(key, out _);

        _memoryCache.Remove(key);

        return Task.FromResult(
            Result.Success<bool, DomainError>(exists));
    }
}