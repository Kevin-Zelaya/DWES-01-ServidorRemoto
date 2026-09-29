using System.Net;

public abstract record DomainError(string message, HttpStatusCode StatusCode, string detail = null);