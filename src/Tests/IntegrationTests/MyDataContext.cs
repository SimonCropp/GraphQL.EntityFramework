using Microsoft.EntityFrameworkCore;

public class IntegrationDbContext :
    DbContext
{
    public DbSet<ParentEntity> ParentEntities { get; set; } = null!;
    public DbSet<FilterParentEntity> FilterParentEntities { get; set; } = null!;
    public DbSet<FilterChildEntity> FilterChildEntities { get; set; } = null!;
    public DbSet<ChildEntity> ChildEntities { get; set; } = null!;
    public DbSet<WithMisNamedQueryChildEntity> WithMisNamedQueryChildEntities { get; set; } = null!;
    public DbSet<WithMisNamedQueryParentEntity> WithMisNamedQueryParentEntities { get; set; } = null!;
    public DbSet<Level1Entity> Level1Entities { get; set; } = null!;
    public DbSet<CustomTypeEntity> CustomTypeEntities { get; set; } = null!;
    public DbSet<Level2Entity> Level2Entities { get; set; } = null!;
    public DbSet<Level3Entity> Level3Entities { get; set; } = null!;
    public DbSet<WithNullableEntity> WithNullableEntities { get; set; } = null!;
    public DbSet<NamedIdEntity> NamedEntities { get; set; } = null!;
    public DbSet<WithManyChildrenEntity> WithManyChildrenEntities { get; set; } = null!;
    public DbSet<Child1Entity> Child1Entities { get; set; } = null!;
    public DbSet<Child2Entity> Child2Entities { get; set; } = null!;
    public DbSet<ParentEntityView> ParentEntityView { get; set; } = null!;

    public IntegrationDbContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentEntityView>()
            .ToView("ParentEntityView")
            .HasNoKey()
            .Property(v => v.Property).HasColumnName("Property");
        modelBuilder.Entity<CustomTypeEntity>();
        modelBuilder.Entity<WithNullableEntity>();
        modelBuilder.Entity<FilterParentEntity>();
        modelBuilder.Entity<FilterChildEntity>();
        modelBuilder.Entity<ParentEntity>();
        modelBuilder.Entity<ChildEntity>();
        modelBuilder.Entity<WithMisNamedQueryParentEntity>();
        modelBuilder.Entity<WithMisNamedQueryChildEntity>();
        modelBuilder.Entity<Level1Entity>();
        modelBuilder.Entity<Level2Entity>();
        modelBuilder.Entity<Level3Entity>();
        modelBuilder.Entity<WithManyChildrenEntity>();
        modelBuilder.Entity<Child1Entity>();
        modelBuilder.Entity<NamedIdEntity>();
        modelBuilder.Entity<Child2Entity>();
    }
}