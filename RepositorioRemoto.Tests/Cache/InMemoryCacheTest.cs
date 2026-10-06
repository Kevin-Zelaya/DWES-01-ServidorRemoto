using NUnit.Framework;
using Microsoft.Extensions.Caching.Memory;
using RepositorioRemoto.Cache;

namespace RepositorioRemoto.Tests.Cache;

public class InMemoryCacheTests
{
    private IMemoryCache _realMemoryCache;
    private InMemoryCache<string> _cache;

    [SetUp]
    public void Setup()
    {
        _realMemoryCache = new MemoryCache(new MemoryCacheOptions());
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryCache<string>>.Instance;
        _cache = new InMemoryCache<string>(_realMemoryCache, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _realMemoryCache.Dispose();
    }

    [Test]
    public async Task GetAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        var key = "non_existent_key";

        var result = await _cache.GetAsync(key);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task SetAsync_ShouldStoreValue_WhenKeyAndValueAreProvided()
    {
        var key = "user_key_1";
        var expectedValue = "Juan Pérez";

        await _cache.SetAsync(key, expectedValue);
        var result = await _cache.GetAsync(key);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task SetAsync_ShouldOverwriteExistingValue_WhenKeyAlreadyExists()
    {
        var key = "user_key_1";
        await _cache.SetAsync(key, "Valor Antiguo");

        await _cache.SetAsync(key, "Valor Actualizado");
        var result = await _cache.GetAsync(key);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("Valor Actualizado"));
    }

    [Test]
    public async Task RemoveAsync_ShouldDeleteValue_WhenKeyExists()
    {
        var key = "user_key_1";
        await _cache.SetAsync(key, "Valor a eliminar");

        await _cache.RemoveAsync(key);
        var result = await _cache.GetAsync(key);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task RemoveAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        var key = "non_existent_key";
        
        var result = await _cache.RemoveAsync(key);

        Assert.That(result.IsSuccess, Is.True);
        
    }

    [Test]
    public async Task SetAsync_ShouldRespectCustomExpiration_WhenProvided()
    {
        var key = "expiring_key";
        var value = "Temporal";
        var customExpiration = TimeSpan.FromMinutes(5);

        await _cache.SetAsync(key, value, customExpiration);
        var result = await _cache.GetAsync(key);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(value));
    }
}