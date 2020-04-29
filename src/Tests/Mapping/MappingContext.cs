using Microsoft.EntityFrameworkCore;

public class MappingContext :
    DbContext
{
    public DbSet<MappingParent> Parents { get; set; } = null!;
    public DbSet<MappingChild> Children { get; set; } = null!;

    public MappingContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MappingParent>();
        modelBuilder.Entity<MappingChild>();
    }
}