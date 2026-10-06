using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using StackExchange.Redis;
using Testcontainers.Redis;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Cache.Common;

[TestFixture]
public class RedisCacheTests
{
    private RedisContainer _container = null!;
    private IConnectionMultiplexer _multiplexer = null!;
    private ICache<string> _cache = null!;
    private string _testPrefix = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new RedisBuilder().WithImage("redis:7-alpine").Build();
        await _container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        _multiplexer = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        var logger = NullLogger<RedisCache<string>>.Instance;
        _cache = new RedisCache<string>(_multiplexer, logger);
        _testPrefix = $"test_{Guid.NewGuid():N}_";
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_multiplexer != null)
        {
            await _multiplexer.DisposeAsync();
        }
    }

    [Test]
    public async Task SetAsync_And_GetAsync_ShouldReturnStoredValue()
    {
        var key = _testPrefix + "key";
        var expectedValue = "Hola Redis";

        var setResult = await _cache.SetAsync(key, expectedValue, TimeSpan.FromMinutes(5));
        Assert.That(setResult.IsSuccess, Is.True);

        var getResult = await _cache.GetAsync(key);
        
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task GetAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        var key = _testPrefix + "non_existent_key";

        var getResult = await _cache.GetAsync(key);

        Assert.That(getResult.IsFailure, Is.True);
    }

    [Test]
    public async Task RemoveAsync_ShouldDeleteValue()
    {
        var key = _testPrefix + "key_to_delete";
        await _cache.SetAsync(key, "Some value");

        var removeResult = await _cache.RemoveAsync(key);
        Assert.That(removeResult.IsSuccess, Is.True);

        var getResult = await _cache.GetAsync(key);
        Assert.That(getResult.IsFailure, Is.True);
    }

    [Test]
    public async Task RemoveAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        var key = _testPrefix + "non_existent_key";

        var removeResult = await _cache.RemoveAsync(key);

        Assert.That(removeResult.IsFailure, Is.True);
    }
}