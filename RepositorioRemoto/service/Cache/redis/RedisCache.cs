using System.Text.Json;
using StackExchange.Redis;
using RepositorioRemoto.Cache.Common;
using CSharpFunctionalExtensions;
using RepositorioRemoto.Errors;

namespace RepositorioRemoto.Cache;

public class RedisCache<T> : ICache<T>
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCache<T>> _logger;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    public RedisCache(IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCache<T>> logger
    )
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
    }

    // ==========================================
    // OBTENER UN ELEMENTO
    // ==========================================
    public async Task<Result<T, DomainError>> GetAsync(string key)
    {
        _logger.LogDebug(
            "Buscando la clave {CacheKey} en la caché.",
            key);
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return Result.Failure<T, DomainError>(
                new CacheError.NotFound(key)
            );
        }
        _logger.LogDebug(
                "Usuario {key} encontrado en caché.", key);
        var jsonString = value.ToString();

        var result = JsonSerializer.Deserialize<T>(jsonString);

        if (result is null)
        {
            return Result.Failure<T, DomainError>(
                new CacheError.NotFound(key)
            );
        }

        return Result.Success<T, DomainError>(result);
    }
    
    // ==========================================
    // GUARDAR UN ELEMENTO
    // ==========================================

    public async Task<Result<bool, DomainError>> SetAsync(
        string key,
        T value,
        TimeSpan? expiration = null)
    {
        var jsonValue = JsonSerializer.Serialize(value);
        var expiry = expiration ?? TimeSpan.FromMinutes(30);

        var response = await _database.StringSetAsync(key, jsonValue, expiry);
        if(response)
            return Result.Success<bool, DomainError>(response);
        
        return Result.Failure<bool, DomainError>(
            new CacheError.NotFound(key)
        );
    }
    // ==========================================
    // ELIMINAR UN ELEMENTO
    // ==========================================

    public async Task<Result<bool, DomainError>> RemoveAsync(string key)
    {
        var response = await _database.KeyDeleteAsync(key);
        if(response)
            return Result.Success<bool, DomainError>(response);
        return Result.Failure<bool, DomainError>(
            new CacheError.NotFound(key)
        );
    }

    public async Task<Result<bool, DomainError>> ClearAsync()
    {
        try
        {
            var endpoints = _connectionMultiplexer.GetEndPoints();
            var server = _connectionMultiplexer.GetServer(endpoints[0]);

            foreach (var key in server.Keys())
            {
                await _database.KeyDeleteAsync(key);
            }

            _logger.LogInformation("Caché de Redis vaciada correctamente.");

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al vaciar la caché de Redis.");

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message));
        }
    }
}