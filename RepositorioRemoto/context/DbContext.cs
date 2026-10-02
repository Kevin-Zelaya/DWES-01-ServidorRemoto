using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }


    public void EnsureCreated()
    {
        Database.EnsureCreated();
    }
}