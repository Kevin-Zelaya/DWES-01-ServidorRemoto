
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
        catch (SqliteException ex) when (ex.SqliteErrorCode is 10 or 14)
        {
            logger.LogError(
                "Error de conexión o acceso al archivo de SQLite.");

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ConnectionFailure(ex.Message));
        }
        catch (SqliteException ex) when (
            ex.SqliteErrorCode == 1 &&
            ex.Message.Contains(
                "no such table",
                StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError(
                "No existe la tabla de usuarios en la base de datos.");

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.SchemaMismatch(ex.Message));
        }
        catch (SqliteException ex)
        {
            logger.LogError(
                "Error de SQLite al obtener todos los usuarios. Código: {SqliteErrorCode}",
                ex.SqliteErrorCode);

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                "La consulta de todos los usuarios fue cancelada.");

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error inesperado al obtener todos los usuarios.");

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    /// <summary>
    /// Obtener un usuario por su ID.
    /// </summary>
    public async Task<Result<UserEntity, DomainError>> GetUserByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Buscando usuario con ID {UserId} en SQLite.", id);

        try
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id == id, ct);

            if (user is null)
            {
                logger.LogWarning(
                    "No se encontró el usuario con ID {UserId}.", id);

                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound("Usuario", id));
            }

            logger.LogInformation(
                "Usuario con ID {UserId} obtenido correctamente.", id);

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                "La consulta del usuario {UserId} fue cancelada.", id);

            throw;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 10 or 14)
        {
            logger.LogError(
                "Error de conexión al consultar el usuario {UserId}.", id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConnectionFailure(ex.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 11 or 26)
        {
            logger.LogCritical(
                "La base de datos SQLite está dañada o no es válida.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.DatabaseCorrupted(ex.Message));
        }
        catch (SqliteException ex)
        {
            logger.LogError(
                "Error de SQLite al consultar el usuario {UserId}. Código: {SqliteErrorCode}",
                id,
                ex.SqliteErrorCode);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error inesperado al consultar el usuario {UserId}.", id);

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
        logger.LogInformation("Creando un usuario en SQLite.");

        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Usuario creado correctamente con ID {UserId}.",
                user.id);

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            logger.LogWarning(
                "No se pudo crear el usuario por una restricción de la base de datos.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6)
        {
            logger.LogWarning(
                "No se pudo crear el usuario porque la base de datos está bloqueada.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.DatabaseLocked(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error al guardar el nuevo usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "La creación del usuario fue cancelada.");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado al crear un usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
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
        {
            logger.LogWarning(
                "No se creó ningún usuario porque la colección está vacía.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(
                    "No se puede crear una colección vacía."));
        }

        logger.LogInformation(
            "Creando {TotalUsers} usuarios en SQLite.", userList.Count);

        try
        {
            context.Users.AddRange(userList);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Se crearon correctamente {TotalUsers} usuarios.",
                userList.Count);

            return Result.Success<bool, DomainError>(true);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            logger.LogWarning(
                "No se pudo crear la colección por una restricción de la base de datos.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6)
        {
            logger.LogWarning(
                "No se pudo crear la colección porque la base de datos está bloqueada.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.DatabaseLocked(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                "Error al guardar la colección de usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                "La creación de la colección de usuarios fue cancelada.");

            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error inesperado al crear la colección de usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    /// <summary>
    /// Actualizar un usuario existente.
    /// </summary>
    public async Task<Result<UserEntity, DomainError>> UpdateAsync(
        UserEntity user,
        int id)
    {
        logger.LogInformation(
            "Actualizando usuario con ID {UserId}.", id);

        try
        {
            var existingUser = await context.Users.FindAsync(id);

            if (existingUser is null)
            {
                logger.LogWarning(
                    "No se encontró el usuario {UserId} para actualizar.", id);

                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound("Usuario", id));
            }

            context.Entry(existingUser).CurrentValues.SetValues(user);

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Usuario con ID {UserId} actualizado correctamente.", id);

            return Result.Success<UserEntity, DomainError>(existingUser);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            logger.LogWarning(
                "No se pudo actualizar el usuario {UserId} por una restricción.",
                id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6)
        {
            logger.LogWarning(
                "La base de datos está bloqueada al actualizar el usuario {UserId}.",
                id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.DatabaseLocked(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                "Error al guardar los cambios del usuario {UserId}.", id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error inesperado al actualizar el usuario {UserId}.", id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    /// <summary>
    /// Eliminar un usuario por su ID.
    /// </summary>
    public async Task<Result<bool, DomainError>> DeleteAsync(int userId)
    {
        logger.LogInformation(
            "Eliminando usuario con ID {UserId}.", userId);

        try
        {
            var user = await context.Users.FindAsync(userId);

            if (user is null)
            {
                logger.LogWarning(
                    "No se encontró el usuario {UserId} para eliminar.",
                    userId);

                return Result.Failure<bool, DomainError>(
                    new DatabaseError.NotFound("Usuario", userId));
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Usuario con ID {UserId} eliminado correctamente.", userId);

            return Result.Success<bool, DomainError>(true);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqliteException
                { SqliteErrorCode: 19 } sqlite)
        {
            logger.LogWarning(
                "No se pudo eliminar el usuario {UserId} por una restricción.",
                userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.ConstraintViolation(sqlite.Message));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(
                "Error al guardar la eliminación del usuario {UserId}.",
                userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error inesperado al eliminar el usuario {UserId}.",
                userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    /// <summary>
    /// Eliminar todos los usuarios.
    /// </summary>
    /// 

    public async Task<Result<bool, DomainError>> DeleteAllAsync(
        CancellationToken ct = default)
    {
        logger.LogInformation("Eliminando todos los usuarios de SQLite.");

        try
        {
            var totalDeleted = await context.Users.ExecuteDeleteAsync(ct);

            logger.LogInformation(
                "Se eliminaron {TotalDeleted} usuarios de SQLite.",
                totalDeleted);

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(
                "La eliminación de todos los usuarios fue cancelada.");

            throw;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 6)
        {
            logger.LogWarning(
                "No se pudieron eliminar los usuarios porque SQLite está bloqueado.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.DatabaseLocked(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(
                "Error inesperado al eliminar todos los usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }


}