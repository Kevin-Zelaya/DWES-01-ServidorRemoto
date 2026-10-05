using System.Net;

namespace RepositorioRemoto.Errors;

public abstract record CacheError(
    string Message,
    HttpStatusCode StatusCode,
    string? Details = null)
    : DomainError(Message, StatusCode, Details)
{
    public sealed record NotFound(string Key)
        : CacheError(
            $"No se encontró la clave '{Key}' en la caché.",
            HttpStatusCode.NotFound);

    public sealed record ConnectionFailure(string Details)
        : CacheError(
            "No se pudo conectar con el servicio de caché.",
            HttpStatusCode.ServiceUnavailable,
            Details);

    public sealed record SerializationFailure(string Details)
        : CacheError(
            "No se pudo serializar o deserializar el valor de la caché.",
            HttpStatusCode.InternalServerError,
            Details);
}