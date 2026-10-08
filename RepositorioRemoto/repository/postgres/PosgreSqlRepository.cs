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

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear el usuario.");

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }


    // Sin log para evitar que se esten ejecutando.
    

    public async Task<Result<bool, DomainError>> CreateRangeAsync(
        IEnumerable<UserEntity> users)
    {
        var usersList = users.ToList();

        if (usersList.Count == 0)
            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(
                    "No se han proporcionado usuarios para crear."));

        try
        {
            await _context.AddRangeAsync(usersList);
            await _context.SaveChangesAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear los usuarios.");

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAllAsync(
        CancellationToken ct = default)
    {
        try
        {
            await _context.Users.ExecuteDeleteAsync(ct);

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar todos los usuarios.");

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
                return Result.Failure<bool, DomainError>(
                    new DatabaseError.NotFound(nameof(UserEntity), userId));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el usuario {UserId}.", userId);

            return Result.Failure<bool, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }
    public async Task<Result<List<UserEntity>, DomainError>> GetAllAsync()
    {
        try
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            return Result.Success<List<UserEntity>, DomainError>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los usuarios.");

            return Result.Failure<List<UserEntity>, DomainError>(
                new DatabaseError.ReadFailure(ex.Message));
        }
    }

    public async Task<Result<UserEntity, DomainError>> GetUserByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.id == id, ct);

            if (user is null)
                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound(nameof(UserEntity), id));

            return Result.Success<UserEntity, DomainError>(user);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el usuario {UserId}.", id);

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
                return Result.Failure<UserEntity, DomainError>(
                    new DatabaseError.NotFound(nameof(UserEntity), id));

            _context.Entry(existingUser)
                .CurrentValues
                .SetValues(user);

            await _context.SaveChangesAsync();

            return Result.Success<UserEntity, DomainError>(existingUser);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el usuario {UserId}.", id);

            return Result.Failure<UserEntity, DomainError>(
                new DatabaseError.WriteFailure(ex.Message));
        }
    }
}