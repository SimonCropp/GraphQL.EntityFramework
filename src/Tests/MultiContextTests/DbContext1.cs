public class DbContext1(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<Entity1> Entities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Entity1>();
}