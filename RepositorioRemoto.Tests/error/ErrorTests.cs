using System.Net;
using NUnit.Framework;
using RepositorioRemoto.Errors;

[TestFixture]
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

    [Test]
    public void DatabaseErrors_ShouldHaveCorrectProperties()
    {
        var connError = new DatabaseError.ConnectionFailure("Detalle conexión");
        Assert.That(connError.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(connError.Details, Is.EqualTo("Detalle conexión"));

        var lockedError = new DatabaseError.DatabaseLocked("Detalle bloqueo");
        Assert.That(lockedError.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));

        var notFoundError = new DatabaseError.NotFound("User", 1);
        Assert.That(notFoundError.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(notFoundError.Message, Does.Contain("User").And.Contain("1"));

        var constraintError = new DatabaseError.ConstraintViolation("Detalle constraint");
        Assert.That(constraintError.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));

        var readError = new DatabaseError.ReadFailure("Detalle lectura");
        Assert.That(readError.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var writeError = new DatabaseError.WriteFailure("Detalle escritura");
        Assert.That(writeError.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var corruptedError = new DatabaseError.DatabaseCorrupted("Detalle corrupción");
        Assert.That(corruptedError.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var schemaError = new DatabaseError.SchemaMismatch("Detalle esquema");
        Assert.That(schemaError.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        var unknownError = new DatabaseError.Unknown("Detalle desconocido");
        Assert.That(unknownError.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public void ApiErrors_ShouldHaveCorrectProperties()
    {
        var notFoundError = new ApiError.NotFound("Product", 42);
        Assert.That(notFoundError.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(notFoundError.Message, Does.Contain("Product").And.Contain("42"));

        var networkError = new ApiError.NetworkError("Detalle red");
        Assert.That(networkError.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(networkError.Details, Is.EqualTo("Detalle red"));

        var timeoutError = new ApiError.Timeout();
        Assert.That(timeoutError.StatusCode, Is.EqualTo(HttpStatusCode.RequestTimeout));

        var httpFailureError = new ApiError.HttpFailure(HttpStatusCode.BadRequest, "Detalle HTTP");
        Assert.That(httpFailureError.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(httpFailureError.Details, Is.EqualTo("Detalle HTTP"));

        var badRequestError = new ApiError.BadRequest();
        Assert.That(badRequestError.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}