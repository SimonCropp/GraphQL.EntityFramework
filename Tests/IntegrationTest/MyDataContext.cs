using Microsoft.EntityFrameworkCore;

public class MyDataContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }

    public MyDataContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>();
    }
}