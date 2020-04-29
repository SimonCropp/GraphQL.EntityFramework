using Microsoft.EntityFrameworkCore;

public class MappingContext :
    DbContext
{
    public DbSet<MappingEntity1> Entities { get; set; } = null!;

    public MappingContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MappingEntity1>();
    }
}