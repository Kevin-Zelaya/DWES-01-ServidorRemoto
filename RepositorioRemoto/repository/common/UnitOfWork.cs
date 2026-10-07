using Npgsql;

public class UnitOfWork(
    AppDbContext _context
) : IUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken cts = default)
    {
        return _context.Database.BeginTransactionAsync(cts);
    }

    public Task CommitTransactionAsync(CancellationToken cts = default)
    {
        return _context.Database.CommitTransactionAsync(cts);
    }

    public async Task RollbackTransactionAsync(
        CancellationToken cts = default)
    {
        try
        {
            if (_context.Database.CurrentTransaction is null)
                return;

            await _context.Database.RollbackTransactionAsync(cts);
        }
        catch (ObjectDisposedException)
        {
            // La transacción ya no está disponible.
        }
        catch (NpgsqlException)
        {
            // La conexión con PostgreSQL se perdió.
        }
}
}