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
        // Levanta el contenedor de Redis automáticamente una sola vez para toda la clase
        _container = new RedisBuilder().WithImage("redis:7-alpine").Build();
        await _container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Destruye y apaga el contenedor al terminar todos los tests
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        // Conecta a la cadena dinámica del contenedor y asigna prefijo único por test
        _multiplexer = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        _cache = new RedisCache<string>(_multiplexer);
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

        await _cache.SetAsync(key, expectedValue, TimeSpan.FromMinutes(5));
        var result = await _cache.GetAsync(key);

        Assert.That(result, Is.EqualTo(expectedValue));
    }

    [Test]
    public async Task RemoveAsync_ShouldDeleteValue()
    {
        var key = _testPrefix + "key_to_delete";
        await _cache.SetAsync(key, "Some value");

        await _cache.RemoveAsync(key);
        var result = await _cache.GetAsync(key);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RemoveAsync_ShouldNotThrow_WhenKeyDoesNotExist()
    {
        var key = _testPrefix + "non_existent_key";

        Assert.That(async () => await _cache.RemoveAsync(key), Throws.Nothing);
    }
}