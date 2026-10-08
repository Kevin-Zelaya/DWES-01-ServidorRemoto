using System.Net;
using RepositorioRemoto.Errors;

public class ErrorTests
{
    [Test]
    public void CacheError_NotFound_ShouldCreateCorrectError()
    {
        var error = new CacheError.NotFound("user:1");

        Assert.That(error.Message, Is.EqualTo(
            "No se encontró la clave 'user:1' en la caché."));

        Assert.That(error.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(error.Key, Is.EqualTo("user:1"));
        Assert.That(error.Details, Is.Null);
    }
    [Test]
    public void CacheError_ConnectionFailure_ShouldCreateCorrectError()
    {
        var error = new CacheError.ConnectionFailure(
            "Redis no responde.");

        Assert.That(error.Message, Is.EqualTo(
            "No se pudo conectar con el servicio de caché."));

        Assert.That(error.StatusCode,
            Is.EqualTo(HttpStatusCode.ServiceUnavailable));

        Assert.That(error.Details,
            Is.EqualTo("Redis no responde."));
    }
    [Test]
    public void CacheError_SerializationFailure_ShouldCreateCorrectError()
    {
        var error = new CacheError.SerializationFailure(
            "JSON inválido.");

        Assert.That(error.Message, Is.EqualTo(
            "No se pudo serializar o deserializar el valor de la caché."));

        Assert.That(error.StatusCode,
            Is.EqualTo(HttpStatusCode.InternalServerError));

        Assert.That(error.Details,
            Is.EqualTo("JSON inválido."));
    }
}