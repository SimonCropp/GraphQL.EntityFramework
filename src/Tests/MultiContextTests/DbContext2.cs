public class DbContext2(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<Entity2> Entities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Entity2>();
}