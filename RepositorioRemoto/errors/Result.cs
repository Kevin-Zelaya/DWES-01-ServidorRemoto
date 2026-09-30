using System.IO.Compression;
public abstract record Result<T, E> where E : DomainError
{
    
    public sealed record Success(T value) : Result<T, E>;

    public sealed record Failure(E error) : Result<T, E>;

    public static Result<T, E> ok(T value) => new Success(value);

    public static Result<T, E> Fail(E error) => new Failure(error);

    public bool IsSuccess => this is Success;
    public bool IsFailure => this is Failure;
}