
using CSharpFunctionalExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class SqliteRepository(AppDbContext context) : IRepository
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
        catch (SqliteException ex) when (ex.SqliteErrorCode is 10 or 14)
        {
            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ConnectionFailure(ex.Message)
            );
        }
        catch (SqliteException ex) when (
        ex.SqliteErrorCode == 1 &&
        ex.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.SchemaMismatch(ex.Message)
            );
        }
        catch(SqliteException ex)
        {
            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ReadFailure(ex.Message)
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.Unknown(ex.Message)
            );
        }
    }

    public async Task<Result<UserEntity, DomainError>> GetUserByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id == id, ct);

            if (user is null)
            {
                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound("Usuario", id));
            }

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 10 or 14)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConnectionFailure(ex.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 11 or 26)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.DatabaseCorrupted(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
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
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.DatabaseLocked(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }
    /// <summary>
    /// Crear un nuevo usuario en la base de datos.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<UserEntity, DomainError>> CreateRangeAsync(
        IEnumerable<UserEntity> users)
    {
        try
        {
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(users.First());
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.DatabaseLocked(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }
    /// <summary>
    /// Actualizar un usuario existente en la base de datos.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<UserEntity, DomainError>> UpdateAsync(
    UserEntity user, int id)
    {
        try
        {
            var existingUser = await context.Users.FindAsync(user.id);

            if (existingUser is null)
            {
                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound("Usuario", user.id));
            }

            context.Entry(existingUser).CurrentValues.SetValues(user);

            await context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(existingUser);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
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
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            return Result.Failure<bool, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }
    public async Task<Result<bool, DomainError>> DeleteAllAsync()
    {
        try
        {
            await context.Users.ExecuteDeleteAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<bool, DomainError>(
                new ApiError.Timeout());
        }
        catch (Exception ex)
        {
            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }
    
}