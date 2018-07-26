using Microsoft.EntityFrameworkCore;

public class MyDataContext : DbContext
{
    public DbSet<ParentEntity> ParentEntities { get; set; }
    public DbSet<ChildEntity> ChildEntities { get; set; }
    public DbSet<WithMisNamedQueryChildEntity> WithMisNamedQueryChildEntities { get; set; }
    public DbSet<WithMisNamedQueryParentEntity> WithMisNamedQueryParentEntities { get; set; }
    public DbSet<Level1Entity> Level1Entities { get; set; }
    public DbSet<Level2Entity> Level2Entities { get; set; }
    public DbSet<Level3Entity> Level3Entities { get; set; }
    public DbSet<WithNullableEntity> WithNullableEntities { get; set; }
    public DbSet<WithManyChildrenEntity> WithManyChildrenEntities { get; set; }
    public DbSet<Child1Entity> Child1Entities { get; set; }
    public DbSet<Child2Entity> Child2Entities { get; set; }

    public MyDataContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WithNullableEntity>();
        modelBuilder.Entity<ParentEntity>();
        modelBuilder.Entity<ChildEntity>();
        modelBuilder.Entity<WithMisNamedQueryParentEntity>();
        modelBuilder.Entity<WithMisNamedQueryChildEntity>();
        modelBuilder.Entity<Level1Entity>();
        modelBuilder.Entity<Level2Entity>();
        modelBuilder.Entity<Level3Entity>();
        modelBuilder.Entity<WithManyChildrenEntity>();
        modelBuilder.Entity<Child1Entity>();
        modelBuilder.Entity<Child2Entity>();
    }
}