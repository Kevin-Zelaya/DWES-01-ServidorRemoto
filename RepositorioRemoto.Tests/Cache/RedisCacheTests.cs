using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using StackExchange.Redis;
using Testcontainers.Redis;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Cache.Common;

[TestFixture]
public class RedisCacheTests
{
    // Variables de apoyo para el contenedor de Redis, la conexión, la caché y un prefijo único por test
    private RedisContainer _container = null!;
    private IConnectionMultiplexer _multiplexer = null!;
    private ICache<string> _cache = null!;
    private string _testPrefix = null!;

    // Configuración inicial que se ejecuta una sola vez antes de todas las pruebas
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Levanta un contenedor Docker efímero con Redis (versión 7-alpine)
        _container = new RedisBuilder().WithImage("redis:7-alpine").Build();
        await _container.StartAsync();
    }

    // Limpieza global que se ejecuta una sola vez al terminar todos los tests
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    // Se ejecuta antes de cada test individual (prepara el terreno)
    [SetUp]
    public async Task SetUp()
    {
        // Conecta al contenedor Redis levantado, instancia el logger y crea la caché de tipo string
        _multiplexer = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        var logger = NullLogger<RedisCache<string>>.Instance;
        _cache = new RedisCache<string>(_multiplexer, logger);
        // Genera un prefijo único para evitar colisiones de claves entre tests
        _testPrefix = $"test_{Guid.NewGuid():N}_";
    }

    // Se ejecuta después de cada test individual (limpieza)
    [TearDown]
    public async Task TearDown()
    {
        if (_multiplexer != null)
        {
            await _multiplexer.DisposeAsync();
        }
    }

    // Comprueba que se puede guardar un valor con un tiempo de expiración y recuperarlo con éxito
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

    // Comprueba que al guardar sin indicar expiración se aplica el valor por defecto (cubre esa rama del código)
    [Test]
    public async Task SetAsync_WithoutExpiration_ShouldUseDefaultExpiry()
    {
        var key = _testPrefix + "default_expiry_key";
        var expectedValue = "Valor sin expiración explícita";

        var setResult = await _cache.SetAsync(key, expectedValue);
        Assert.That(setResult.IsSuccess, Is.True);

        var getResult = await _cache.GetAsync(key);
        Assert.That(getResult.IsSuccess, Is.True);
        Assert.That(getResult.Value, Is.EqualTo(expectedValue));
    }

    // Verifica que buscar una clave que no existe devuelve un fallo (NotFound)
    [Test]
    public async Task GetAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        var key = _testPrefix + "non_existent_key";

        var getResult = await _cache.GetAsync(key);

        Assert.That(getResult.IsFailure, Is.True);
    }

    // Comprueba que se puede eliminar una clave existente correctamente de la caché
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

    // Verifica que intentar borrar una clave inexistente devuelve un fallo
    [Test]
    public async Task RemoveAsync_ShouldReturnFailure_WhenKeyDoesNotExist()
    {
        var key = _testPrefix + "non_existent_key";

        var removeResult = await _cache.RemoveAsync(key);

        Assert.That(removeResult.IsFailure, Is.True);
    }

    // Llena la caché con varias claves y comprueba que ClearAsync() las borra todas por completo
    [Test]
    public async Task ClearAsync_ShouldFlushAllKeysSuccessfully()
    {
        var key1 = _testPrefix + "key1";
        var key2 = _testPrefix + "key2";

        await _cache.SetAsync(key1, "Valor 1");
        await _cache.SetAsync(key2, "Valor 2");

        var clearResult = await _cache.ClearAsync();
        Assert.That(clearResult.IsSuccess, Is.True);

        var getResult1 = await _cache.GetAsync(key1);
        Assert.That(getResult1.IsFailure, Is.True);
    }
}