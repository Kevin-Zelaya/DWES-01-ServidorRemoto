
using System.Net;
using Microsoft.Extensions.FileProviders;

public abstract record ApiError(string Message, HttpStatusCode StatusCode, string? Details = null) 
    : DomainError(Message, StatusCode, Details)
{
    // Records anidados especificos de la API
    public record NotFound(string EntityName, object Id) 
        : ApiError($"El recurso '{EntityName}' con ID '{Id}' no fue encontrado en la API.", HttpStatusCode.NotFound);

    public record NetworkError(string Details) 
        : ApiError("Error de red al conectar con la API remota.", HttpStatusCode.ServiceUnavailable, Details);

    public record Timeout(string Details = "La petición a la API excedió el tiempo límite.") 
        : ApiError(Details, HttpStatusCode.RequestTimeout);

    public record HttpFailure(HttpStatusCode StatusCode, string Details) 
        : ApiError($"La API remota respondió con código {StatusCode}.", StatusCode, Details);

    public record BadRequest(string Details = "La solicitud enviada a la API externa no es válida o tiene un formato incorrecto.") 
        : ApiError("Solicitud incorrecta a la API remota (Bad Request).", HttpStatusCode.BadRequest, Details);
}