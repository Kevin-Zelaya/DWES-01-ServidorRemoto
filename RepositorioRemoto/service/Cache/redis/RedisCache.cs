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

    public RedisCache(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCache<T>> logger)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<Result<T, DomainError>> GetAsync(string key)
    {
        try
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
                "Usuario {key} encontrado en caché.",
                key);

            var result = JsonSerializer.Deserialize<T>(value.ToString());

            if (result is null)
            {
                return Result.Failure<T, DomainError>(
                    new CacheError.NotFound(key)
                );
            }

            return Result.Success<T, DomainError>(result);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis no está disponible al obtener la clave {CacheKey}.",
                key);

            return Result.Failure<T, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(
                ex,
                "Timeout al consultar Redis para la clave {CacheKey}.",
                key);

            return Result.Failure<T, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
    }

    public async Task<Result<bool, DomainError>> SetAsync(
        string key,
        T value,
        TimeSpan? expiration = null)
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var expiry = expiration ?? TimeSpan.FromMinutes(30);

            var response = await _database.StringSetAsync(
                key,
                jsonValue,
                expiry);

            if (response)
            {
                return Result.Success<bool, DomainError>(true);
            }

            return Result.Failure<bool, DomainError>(
                new CacheError.NotFound(key)
            );
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis no está disponible al guardar la clave {CacheKey}.",
                key);

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(
                ex,
                "Timeout al guardar la clave {CacheKey} en Redis.",
                key);

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
    }

    public async Task<Result<bool, DomainError>> RemoveAsync(string key)
    {
        try
        {
            var response = await _database.KeyDeleteAsync(key);

            if (response)
            {
                return Result.Success<bool, DomainError>(true);
            }

            return Result.Failure<bool, DomainError>(
                new CacheError.NotFound(key)
            );
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis no está disponible al eliminar la clave {CacheKey}.",
                key);

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(
                ex,
                "Timeout al eliminar la clave {CacheKey} de Redis.",
                key);

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
    }

    public async Task<Result<bool, DomainError>> ClearAsync()
    {
        try
        {
            var endpoints = _connectionMultiplexer.GetEndPoints();

            if (endpoints.Length == 0)
            {
                return Result.Failure<bool, DomainError>(
                    new CacheError.ConnectionFailure(
                        "No hay endpoints de Redis disponibles.")
                );
            }

            var server = _connectionMultiplexer.GetServer(endpoints[0]);

            foreach (var key in server.Keys())
            {
                await _database.KeyDeleteAsync(key);
            }

            _logger.LogInformation(
                "Caché de Redis vaciada correctamente.");

            return Result.Success<bool, DomainError>(true);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis no está disponible al vaciar la caché.");

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning(
                ex,
                "Timeout al vaciar la caché de Redis.");

            return Result.Failure<bool, DomainError>(
                new CacheError.ConnectionFailure(ex.Message)
            );
        }
    }
}