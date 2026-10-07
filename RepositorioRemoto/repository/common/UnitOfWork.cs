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
    public Task RollbackTransactionAsync(CancellationToken cts = default)
    {
        return _context.Database.RollbackTransactionAsync(cts);
    }
}