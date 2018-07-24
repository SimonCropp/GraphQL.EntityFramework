using Microsoft.EntityFrameworkCore;

public class MyDataContext : DbContext
{
    public DbSet<ParentEntity> ParentEntities { get; set; }
    public DbSet<ChildEntity> ChildEntities { get; set; }

    public MyDataContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentEntity>();
        modelBuilder.Entity<ChildEntity>();
    }
}