
using CSharpFunctionalExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class SqliteRepository(
    AppDbContext context,
    ILogger<SqliteRepository> logger
) : IRepository
{
    /// <summary>
    /// Obtener todos los usuarios de la base de datos.
    /// </summary>
    public async Task<Result<List<UserEntity>, DomainError>> GetAllAsync()
    {
        logger.LogInformation("Obteniendo todos los usuarios de SQLite.");

        try
        {
            var users = await context.Users
                .AsNoTracking()
                .ToListAsync();

            logger.LogInformation(
                "Se obtuvieron {TotalUsers} usuarios de SQLite.",
                users.Count);

            return Result.Success<List<UserEntity>, DomainError>(users);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("La consulta de usuarios fue cancelada.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener los usuarios de SQLite.");

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
    }

    /// <summary>
    /// Obtener un usuario por su ID.
    /// </summary>

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
                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound("Usuario", id));

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SqliteException ex)
        {
            logger.LogError(ex, "Error de SQLite al obtener el usuario {UserId}.", id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener el usuario {UserId}.", id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
    }


    /// <summary>
    /// Crear un usuario en la base de datos.
    /// </summary>
    public async Task<Result<UserEntity, DomainError>> CreateAsync(
        UserEntity user)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear el usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }

    /// <summary>
    /// Crear varios usuarios en una sola operación.
    /// </summary>
   public async Task<Result<bool, DomainError>> CreateRangeAsync(
        IEnumerable<UserEntity> users)
    {
        var userList = users.ToList();

        if (userList.Count == 0)
            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(
                    "No se puede crear una colección vacía."));

        try
        {
            context.Users.AddRange(userList);
            await context.SaveChangesAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al crear {TotalUsers} usuarios.",
                userList.Count);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }

    /// <summary>
    /// Actualizar un usuario existente.
    /// </summary>
    public async Task<Result<UserEntity, DomainError>> UpdateAsync(
        UserEntity user,
        int id)
    {
        try
        {
            var existingUser = await context.Users.FindAsync(id);

            if (existingUser is null)
                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound("Usuario", id));

            context.Entry(existingUser).CurrentValues.SetValues(user);

            await context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(existingUser);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al actualizar el usuario {UserId}.",
                id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }

    /// <summary>
    /// Eliminar un usuario por su ID.
    /// </summary>
   public async Task<Result<bool, DomainError>> DeleteAsync(int userId)
    {
        try
        {
            var user = await context.Users.FindAsync(userId);

            if (user is null)
                return Result.Failure<bool, DomainError>(
                    new DatabaseError.NotFound("Usuario", userId));

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al eliminar el usuario {UserId}.",
                userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }

    /// <summary>
    /// Eliminar todos los usuarios.
    /// </summary>
    /// 

    public async Task<Result<bool, DomainError>> DeleteAllAsync(
        CancellationToken ct = default)
    {
        try
        {
            await context.Users.ExecuteDeleteAsync(ct);
            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al eliminar todos los usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }


}