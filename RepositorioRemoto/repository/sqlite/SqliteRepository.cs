
using CSharpFunctionalExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class SqliteRepository(AppDbContext context)
{
    /// <summary>
    /// Obtener todos los usuarios de la base de datos.
    /// </summary>
    /// <returns></returns>
    public async Task<Result<List<UserEntity>, DomainError>> GetAllAsync()
    {
        try
        {
            var users = await context.Users
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<List<UserEntity>, DomainError>(users);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserEntity>, DomainError>(
                MapError(ex, "read"));
        }
    }

    /// <summary>
    /// Crear un nuevo usuario en la base de datos.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<UserEntity, DomainError>> CreateAsync(
        UserEntity user)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                MapError(ex, "write"));
        }
    }
    /// <summary>
    /// Actualizar un usuario existente en la base de datos.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<UserEntity, DomainError>> UpdateAsync(
        UserEntity user)
    {
        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                MapError(ex, "write"));
        }
    }

    /// <summary>
    /// Eliminar un usuario de la base de datos.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Result<bool, DomainError>> DeleteAsync(int userId)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);

            if (user is null)
            {
                return Result.Failure<bool, DomainError>(
                    new DatabaseError.NotFound("Usuario", userId));
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(
                MapError(ex, "write"));
        }
    }
    /// <summary>
    /// Mapear excepciones de SQLite a errores de dominio.
    /// </summary>
    private static DatabaseError MapError(Exception ex, string operation)
    {
        // EF Core puede envolver el error de SQLite.
        var sqliteEx = ex switch
        {
            SqliteException sqlite => sqlite,
            DbUpdateException { InnerException: SqliteException sqlite } => sqlite,
            _ => null
        };

        if (sqliteEx is not null)
        {
            return sqliteEx.SqliteErrorCode switch
            {
                5 or 6 => new DatabaseError.DatabaseLocked(ex.Message),

                10 or 14 => new DatabaseError.ConnectionFailure(ex.Message),

                11 or 26 => new DatabaseError.DatabaseCorrupted(ex.Message),

                19 => new DatabaseError.ConstraintViolation(ex.Message),

                1 when ex.Message.Contains(
                    "no such table",
                    StringComparison.OrdinalIgnoreCase)
                    => new DatabaseError.SchemaMismatch(ex.Message),

                _ => operation == "read"
                    ? new DatabaseError.ReadFailure(ex.Message)
                    : new DatabaseError.WriteFailure(ex.Message)
            };
        }

        if (ex is DbUpdateException)
        {
            return new DatabaseError.WriteFailure(ex.Message);
        }

        return operation == "read"
            ? new DatabaseError.ReadFailure(ex.Message)
            : new DatabaseError.Unknown(ex.Message);
    }
}