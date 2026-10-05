using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RepositorioRemoto.Cache.Common;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Cache;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class InMemoryCache<T>(
    IMemoryCache memoryCache,
    ILogger<InMemoryCache<T>> logger
) : ICache<T>
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<InMemoryCache<T>> _logger = logger;

    // ==========================================
    // OBTENER UN ELEMENTO
    // ==========================================
    public Task<Result<T, DomainError>> GetAsync(string key)
    {
        _logger.LogDebug(
            "Buscando la clave {CacheKey} en la caché.",
            key);

        if (_memoryCache.TryGetValue(key, out T? value)
            && value is not null)
        {
            _logger.LogDebug(
                "Elemento encontrado en caché para la clave {CacheKey}.",
                key);

            return Task.FromResult(
                Result.Success<T, DomainError>(value));
        }

        _logger.LogDebug(
            "No se encontró la clave {CacheKey} en la caché.",
            key);

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
        var duration = expiration ?? TimeSpan.FromMinutes(30);

        _logger.LogDebug(
            "Guardando la clave {CacheKey} en caché. Expiración: {Expiration}.",
            key,
            duration);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = duration
        };

        _memoryCache.Set(key, value, options);

        _logger.LogDebug(
            "Clave {CacheKey} guardada correctamente en caché.",
            key);

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

        if (exists)
        {
            _logger.LogDebug(
                "Clave {CacheKey} eliminada de la caché.",
                key);
        }
        else
        {
            _logger.LogDebug(
                "No se eliminó ninguna entrada: la clave {CacheKey} no existía.",
                key);
        }

        return Task.FromResult(
            Result.Success<bool, DomainError>(exists));
    }
}