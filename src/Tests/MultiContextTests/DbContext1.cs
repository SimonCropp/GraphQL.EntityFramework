using Microsoft.EntityFrameworkCore;

public class DbContext1 :
    DbContext
{
    public DbSet<Entity1> Entities { get; set; } = null!;

    public DbContext1(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity1>();
    }
}