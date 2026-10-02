```csharp
using System.Net;

public abstract record DatabaseError(
    string Message,
    HttpStatusCode StatusCode,
    string? Details = null)
    : DomainError(Message, StatusCode, Details)
{
    public record ConnectionFailure(string Details)
        : DatabaseError(
            "No se pudo establecer la conexión con la base de datos.",
            HttpStatusCode.ServiceUnavailable,
            Details);

    public record DatabaseLocked(string Details)
        : DatabaseError(
            "La base de datos está bloqueada y no puede procesar la operación.",
            HttpStatusCode.ServiceUnavailable,
            Details);

    public record NotFound(string EntityName, object Id)
        : DatabaseError(
            $"El recurso '{EntityName}' con ID '{Id}' no fue encontrado en la base de datos.",
            HttpStatusCode.NotFound);

    public record ConstraintViolation(string Details)
        : DatabaseError(
            "La operación incumple una restricción de la base de datos.",
            HttpStatusCode.Conflict,
            Details);

    public record ReadFailure(string Details)
        : DatabaseError(
            "Error al leer los datos de la base de datos.",
            HttpStatusCode.InternalServerError,
            Details);

    public record WriteFailure(string Details)
        : DatabaseError(
            "Error al guardar los datos en la base de datos.",
            HttpStatusCode.InternalServerError,
            Details);

    public record DatabaseCorrupted(string Details)
        : DatabaseError(
            "La base de datos presenta un problema de integridad.",
            HttpStatusCode.InternalServerError,
            Details);

    public record SchemaMismatch(string Details)
        : DatabaseError(
            "La estructura de la base de datos no coincide con la esperada.",
            HttpStatusCode.InternalServerError,
            Details);

    public record Unknown(string Details)
        : DatabaseError(
            "Se ha producido un error inesperado en la base de datos.",
            HttpStatusCode.InternalServerError,
            Details);

}