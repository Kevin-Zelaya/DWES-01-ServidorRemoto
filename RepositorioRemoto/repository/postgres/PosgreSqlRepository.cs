using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

public class PostgreSqlRepository(
    ILogger<PostgreSqlRepository> _logger,
    AppDbContext _context
) : IRepository
{



    /// Postgres códigos de estado.
    // 23505 - key duplicada
    // 23503 - foreingkey
    // 23502 - valor null donde no se puede
    // 42P01 - tabla que no existe
    // 42703 - columna que no exite
    // 08000 - Error de conexión
    // 08003 - Conección inexistente
    // 08006 - otro fallo de conección
    public async Task<Result<UserEntity, DomainError>> CreateAsync(
    UserEntity user)
    {
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Usuario creado correctamente. Id: {UserId}",
                user.id);

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx
                && pgEx.SqlState is "23505" or "23503" or "23502" or "23514")
        {
            _logger.LogWarning(
                ex,
                "Violación de restricción al crear el usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConstraintViolation(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Error al guardar el usuario en PostgreSQL.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Creación del usuario cancelada.");
            throw;
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(
                ex,
                "Error de PostgreSQL al crear el usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ConnectionFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al crear el usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }


    // Sin log para evitar que se esten ejecutando.
    public async Task<Result<bool, DomainError>> CreateRangeAsync(
        IEnumerable<UserEntity> users)
    {
        try
        {
            var usersList = users.ToList();

            if (usersList.Count == 0)
            {
                _logger.LogWarning(
                    "Lote de usuarios vacios.");

                return Result.Failure<bool, DomainError>(
                    new DatabaseError.WriteFailure(
                        "No se han proporcionado usuarios para crear."));
            }

            // Añadir usuarios al contexto
            await _context.AddRangeAsync(usersList);

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            // _logger.LogInformation(
            //     "Se han creado {UserCount} usuarios correctamente.",
            //     usersList.Count);

            return Result.Success<bool, DomainError>(
                true);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pgEx
                && pgEx.SqlState is "23505" or "23503" or "23502" or "23514")
        {
            // _logger.LogWarning(
            //     ex,
            //     "Se ha producido una violación de restricciones al crear usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.ConstraintViolation(ex.Message));
        }
        catch (DbUpdateException ex)
        {
            // _logger.LogError(
            //     ex,
            //     "Error al guardar el lote de usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (OperationCanceledException)
        {
            // _logger.LogWarning(
            //     "Se ha cancelado la creación del lote de usuarios.");

            throw;
        }
        catch (Exception ex)
        {
            // _logger.LogError(
            //     ex,
            //     "Error inesperado al crear el lote de usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAllAsync(
        CancellationToken cts = default)
    {
        try
        {
            var deleted = await _context.Users.ExecuteDeleteAsync(cts);

            _logger.LogInformation(
                "Se han eliminado {Count} usuarios",
                deleted);

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Eliminación de todos los usuarios cancelada");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar todos los usuarios");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);

            if (user is null)
            {
                _logger.LogWarning(
                    "No se encontró el usuario {UserId} para eliminar",
                    userId);

                return Result.Failure<bool, DomainError>(
                    new DatabaseError.NotFound(nameof(UserEntity), userId));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Usuario {UserId} eliminado correctamente",
                userId);

            return Result.Success<bool, DomainError>(true);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Error al eliminar el usuario {UserId}",
                userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al eliminar el usuario {UserId}",
                userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }

    public async Task<Result<List<UserEntity>, DomainError>> GetAllAsync()
    {
        try
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation(
                "Se han obtenido {Count} usuarios",
                users.Count);

            return Result.Success<List<UserEntity>, DomainError>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los usuarios");

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
    }

    public async Task<Result<UserEntity, DomainError>> GetUserByIdAsync(
        int id,
        CancellationToken cts = default)
    {
        try
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id == id, cts);

            if (user is null)
            {
                _logger.LogWarning(
                    "No se encontró el usuario {UserId}",
                    id);

                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound(nameof(UserEntity), id));
            }

            _logger.LogInformation(
                "Usuario {UserId} obtenido correctamente",
                id);

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Consulta del usuario {UserId} cancelada",
                id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al consultar el usuario {UserId}",
                id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
    }

    public async Task<Result<UserEntity, DomainError>> UpdateAsync(
        UserEntity user,
        int id)
    {
        try
        {
            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser is null)
            {
                _logger.LogWarning(
                    "No se encontró el usuario {UserId} para actualizar.",
                    id);

                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound(nameof(UserEntity), id));
            }

            _context.Entry(existingUser)
                .CurrentValues
                .SetValues(user);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Usuario {UserId} actualizado correctamente.",
                id);

            return Result.Success<UserEntity, DomainError>(existingUser);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Error al guardar los cambios del usuario {UserId}.",
                id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error inesperado al actualizar el usuario {UserId}.",
                id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.Unknown(ex.Message));
        }
    }
}