using Microsoft.EntityFrameworkCore;

public class DependencyDbContext :
    DbContext
{
    public DbSet<Entity> Entities { get; set; } = null!;

    public DependencyDbContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>();
    }
}