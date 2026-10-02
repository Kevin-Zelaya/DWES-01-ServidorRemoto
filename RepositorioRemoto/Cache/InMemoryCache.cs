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
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public InMemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    // ==========================================
    // OBTENER UN ELEMENTO
    // ==========================================
    public async Task<T?> GetAsync(string key)
    {
        await _cacheLock.WaitAsync();

        try
        {
            _memoryCache.TryGetValue(key, out T? value);
            return value;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    // ==========================================
    // GUARDAR UN ELEMENTO
    // ==========================================
    public async Task SetAsync(
        string key,
        T value,
        TimeSpan? expiration = null)
    {
        await _cacheLock.WaitAsync();

        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    expiration ?? TimeSpan.FromMinutes(30)
            };

            _memoryCache.Set(key, value, options);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    // ==========================================
    // ELIMINAR UN ELEMENTO
    // ==========================================
    public async Task RemoveAsync(string key)
    {
        await _cacheLock.WaitAsync();

        try
        {
            _memoryCache.Remove(key);
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
