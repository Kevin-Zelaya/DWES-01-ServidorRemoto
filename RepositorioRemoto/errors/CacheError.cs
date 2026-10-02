using System.Net;

namespace RepositorioRemoto.Errors;

public abstract record CacheError(string Message, HttpStatusCode StatusCode, string? Details = null) 
    : DomainError(Message, StatusCode, Details)
{
    // Elemento no encontrado en la memoria local
    public record NotFound(string EntityName, object Id) 
        : CacheError($"El recurso '{EntityName}' con ID '{Id}' no fue encontrado en la caché local.", HttpStatusCode.NotFound);

    // Conflicto por intentar crear un elemento que ya existe
    public record AlreadyExists(string EntityName, object Id) 
        : CacheError($"El recurso '{EntityName}' con ID '{Id}' ya existe en la caché.", HttpStatusCode.Conflict);

    // Conflicto de concurrencia o modificación simultánea
    public record ConcurrencyConflict(string Details = "Se produjo un conflicto de concurrencia al intentar modificar la caché.") 
        : CacheError(Details, HttpStatusCode.Conflict);

    // Operación no válida sobre la estructura de la caché
    public record InvalidOperation(string Details) 
        : CacheError("Operación inválida en la caché local.", HttpStatusCode.BadRequest, Details);
}