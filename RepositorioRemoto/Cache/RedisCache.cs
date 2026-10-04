using System.Text.Json;
using StackExchange.Redis;
using RepositorioRemoto.Cache.Common;

namespace RepositorioRemoto.Cache;

public class RedisCache<T> : ICache<T>
{
    private readonly IDatabase _database;

    public RedisCache(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    // ==========================================
    // OBTENER UN ELEMENTO
    // ==========================================
    public async Task<T?> GetAsync(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return default;
        }

        // Convertimos explícitamente a string para evitar la ambigüedad en el Deserialize
        string jsonString = value.ToString();
        return JsonSerializer.Deserialize<T>(jsonString);
    }
    
    // ==========================================
    // GUARDAR UN ELEMENTO
    // ==========================================

    public async Task SetAsync(
        string key,
        T value,
        TimeSpan? expiration = null)
    {
        var jsonValue = JsonSerializer.Serialize(value);
        var expiry = expiration ?? TimeSpan.FromMinutes(30);

        await _database.StringSetAsync(key, jsonValue, expiry);
    }
    // ==========================================
    // ELIMINAR UN ELEMENTO
    // ==========================================

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }
}