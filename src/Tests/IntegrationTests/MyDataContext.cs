public class IntegrationDbContext(DbContextOptions options) :
    DbContext(options)
{
    public DbSet<ParentEntity> ParentEntities { get; set; } = null!;
    public DbSet<DateEntity> DateEntities { get; set; } = null!;
    public DbSet<EnumEntity> EnumEntities { get; set; } = null!;
    public DbSet<StringEntity> StringEntities { get; set; } = null!;
    public DbSet<TimeEntity> TimeEntities { get; set; } = null!;
    public DbSet<FilterParentEntity> FilterParentEntities { get; set; } = null!;
    public DbSet<FilterChildEntity> FilterChildEntities { get; set; } = null!;
    public DbSet<ChildEntity> ChildEntities { get; set; } = null!;
    public DbSet<WithMisNamedQueryChildEntity> WithMisNamedQueryChildEntities { get; set; } = null!;
    public DbSet<WithMisNamedQueryParentEntity> WithMisNamedQueryParentEntities { get; set; } = null!;
    public DbSet<Level1Entity> Level1Entities { get; set; } = null!;
    public DbSet<CustomTypeEntity> CustomTypeEntities { get; set; } = null!;
    public DbSet<IncludeNonQueryableB> IncludeNonQueryableBs { get; set; } = null!;
    public DbSet<IncludeNonQueryableA> IncludeNonQueryableAs { get; set; } = null!;
    public DbSet<Level2Entity> Level2Entities { get; set; } = null!;
    public DbSet<Level3Entity> Level3Entities { get; set; } = null!;
    public DbSet<WithNullableEntity> WithNullableEntities { get; set; } = null!;
    public DbSet<NamedIdEntity> NamedEntities { get; set; } = null!;
    public DbSet<WithManyChildrenEntity> WithManyChildrenEntities { get; set; } = null!;
    public DbSet<Child1Entity> Child1Entities { get; set; } = null!;
    public DbSet<Child2Entity> Child2Entities { get; set; } = null!;
    public DbSet<ParentEntityView> ParentEntityView { get; set; } = null!;
    public DbSet<InheritedEntity> InheritedEntities { get; set; } = null!;
    public DbSet<ManyToManyLeftEntity> ManyToManyLeftEntities { get; set; } = null!;
    public DbSet<ManyToManyRightEntity> ManyToManyRightEntities { get; set; } = null!;
    public DbSet<ManyToManyMiddleEntity> ManyToManyMiddleEntities { get; set; } = null!;
    public DbSet<ManyToManyShadowLeftEntity> ManyToManyShadowLeftEntities { get; set; } = null!;
    public DbSet<ManyToManyShadowRightEntity> ManyToManyShadowRightEntities { get; set; } = null!;
    public DbSet<OwnedParent> OwnedParents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentEntityView>()
            .ToView("ParentEntityView")
            .HasNoKey()
            .Property(_ => _.Property).HasColumnName("Property");
        modelBuilder.Entity<CustomTypeEntity>();
        modelBuilder.Entity<WithNullableEntity>();
        modelBuilder.Entity<FilterParentEntity>();
        modelBuilder.Entity<FilterChildEntity>();
        modelBuilder.Entity<ParentEntity>();
        modelBuilder.Entity<ChildEntity>();
        modelBuilder.Entity<WithMisNamedQueryParentEntity>();
        modelBuilder.Entity<WithMisNamedQueryChildEntity>();
        modelBuilder.Entity<IncludeNonQueryableB>();
        modelBuilder.Entity<IncludeNonQueryableA>()
            .HasOne(_ => _.IncludeNonQueryableB)
            .WithOne(_ => _.IncludeNonQueryableA)
            .HasForeignKey<IncludeNonQueryableB>(_ => _.IncludeNonQueryableAId);
        modelBuilder.Entity<Level1Entity>();
        modelBuilder.Entity<Level2Entity>();
        modelBuilder.Entity<Level3Entity>();
        modelBuilder.Entity<WithManyChildrenEntity>();
        modelBuilder.Entity<Child1Entity>();
        modelBuilder.Entity<NamedIdEntity>();
        modelBuilder.Entity<OwnedParent>();
        modelBuilder.Entity<Child2Entity>();
        modelBuilder.Entity<DerivedEntity>().HasBaseType<InheritedEntity>();
        modelBuilder.Entity<DerivedWithNavigationEntity>()
            .HasBaseType<InheritedEntity>()
            .HasMany(_ => _.Children)
            .WithOne(_ => _.TypedParent!)
            .HasForeignKey(_ => _.TypedParentId);
        modelBuilder.Entity<ManyToManyRightEntity>()
            .HasMany(_ => _.Lefts)
            .WithMany(l => l.Rights)
            .UsingEntity<ManyToManyMiddleEntity>(
                _ => _.HasOne(_ => _.ManyToManyLeftEntity).WithMany(),
                _ => _.HasOne(_ => _.ManyToManyRightEntity).WithMany());
        modelBuilder.Entity<ManyToManyShadowLeftEntity>()
            .HasMany(x=> x.ManyToManyShadowRightEntities)
            .WithMany(x=> x.ManyToManyShadowLeftEntities)
            .UsingEntity("ManyToManyShadowMiddleEntity");
    }
}