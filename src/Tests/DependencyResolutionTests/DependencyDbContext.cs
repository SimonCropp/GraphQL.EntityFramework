public class DependencyDbContext(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<Entity> Entities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Entity>();
}