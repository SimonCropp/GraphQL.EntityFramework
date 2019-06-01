using Microsoft.EntityFrameworkCore;

public class DbContext2 :
    DbContext
{
    public DbSet<Entity2> Entities { get; set; }

    public DbContext2(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity2>();
    }
}