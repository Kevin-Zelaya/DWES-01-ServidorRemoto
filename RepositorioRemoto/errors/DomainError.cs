using System.Net;
public record  DomainError(string message, HttpStatusCode StatusCode, string? detail = null);