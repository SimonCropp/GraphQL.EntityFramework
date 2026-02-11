public class IntegrationDbContext(DbContextOptions options) :
    DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder builder) =>
        builder.UseDefaultOrderBy();

    public DbSet<ParentEntity> ParentEntities { get; set; } = null!;
    public DbSet<DateEntity> DateEntities { get; set; } = null!;
    public DbSet<EnumEntity> EnumEntities { get; set; } = null!;
    public DbSet<StringEntity> StringEntities { get; set; } = null!;
    public DbSet<TimeEntity> TimeEntities { get; set; } = null!;
    public DbSet<FilterParentEntity> FilterParentEntities { get; set; } = null!;
    public DbSet<FilterChildEntity> FilterChildEntities { get; set; } = null!;
    public DbSet<SimpleTypeFilterEntity> SimpleTypeFilterEntities { get; set; } = null!;
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
    public DbSet<BaseEntity> BaseEntities { get; set; } = null!;
    public DbSet<DerivedChildEntity> DerivedChildEntities { get; set; } = null!;
    public DbSet<ManyToManyLeftEntity> ManyToManyLeftEntities { get; set; } = null!;
    public DbSet<ManyToManyRightEntity> ManyToManyRightEntities { get; set; } = null!;
    public DbSet<ManyToManyMiddleEntity> ManyToManyMiddleEntities { get; set; } = null!;
    public DbSet<ManyToManyShadowLeftEntity> ManyToManyShadowLeftEntities { get; set; } = null!;
    public DbSet<ManyToManyShadowRightEntity> ManyToManyShadowRightEntities { get; set; } = null!;
    public DbSet<OwnedParent> OwnedParents { get; set; } = null!;
    public DbSet<ReadOnlyEntity> ReadOnlyEntities { get; set; } = null!;
    public DbSet<ReadOnlyParentEntity> ReadOnlyParentEntities { get; set; } = null!;
    public DbSet<FieldBuilderProjectionEntity> FieldBuilderProjectionEntities { get; set; } = null!;
    public DbSet<FieldBuilderProjectionParentEntity> FieldBuilderProjectionParentEntities { get; set; } = null!;
    public DbSet<DepartmentEntity> Departments { get; set; } = null!;
    public DbSet<EmployeeEntity> Employees { get; set; } = null!;
    public DbSet<FilterBaseEntity> FilterBaseEntities { get; set; } = null!;
    public DbSet<FilterDerivedEntity> FilterDerivedEntities { get; set; } = null!;
    public DbSet<FilterReferenceEntity> FilterReferenceEntities { get; set; } = null!;
    public DbSet<DiscriminatorBaseEntity> DiscriminatorBaseEntities { get; set; } = null!;
    public DbSet<DiscriminatorDerivedAEntity> DiscriminatorDerivedAEntities { get; set; } = null!;
    public DbSet<DiscriminatorDerivedBEntity> DiscriminatorDerivedBEntities { get; set; } = null!;
    public DbSet<TphRootEntity> TphRootEntities { get; set; } = null!;
    public DbSet<TphMiddleEntity> TphMiddleEntities { get; set; } = null!;
    public DbSet<TphLeafEntity> TphLeafEntities { get; set; } = null!;
    public DbSet<TphAttachmentEntity> TphAttachmentEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentEntityView>()
            .ToView("ParentEntityView")
            .HasNoKey()
            .Property(_ => _.Property).HasColumnName("Property");
        modelBuilder.Entity<CustomTypeEntity>();
        modelBuilder.Entity<WithNullableEntity>();
        modelBuilder.Entity<FilterParentEntity>().OrderBy(_ => _.Property);
        modelBuilder.Entity<FilterChildEntity>().OrderBy(_ => _.Property);
        modelBuilder.Entity<SimpleTypeFilterEntity>().OrderBy(_ => _.Name);
        modelBuilder.Entity<ParentEntity>()
            .OrderBy(_ => _.Property);
        modelBuilder.Entity<ChildEntity>()
            .OrderBy(_ => _.Property);
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
        modelBuilder.Entity<BaseEntity>()
            .OrderBy(_ => _.Property);
        modelBuilder.Entity<DerivedEntity>()
            .HasBaseType<BaseEntity>()
            .OrderByDescending(_ => _.Property);
        modelBuilder.Entity<DerivedWithNavigationEntity>()
            .HasBaseType<BaseEntity>()
            .HasMany(_ => _.Children)
            .WithOne(_ => _.TypedParent!)
            .HasForeignKey(_ => _.TypedParentId);
        var derivedChildEntity = modelBuilder.Entity<DerivedChildEntity>();
        derivedChildEntity
            .HasOne(_ => _.Parent)
            .WithMany(_ => _.ChildrenFromBase)
            .HasForeignKey(_ => _.ParentId);
        derivedChildEntity.OrderBy(_ => _.Property);
        modelBuilder.Entity<ManyToManyRightEntity>()
            .HasMany(_ => _.Lefts)
            .WithMany(_ => _.Rights)
            .UsingEntity<ManyToManyMiddleEntity>(
                _ => _.HasOne(_ => _.ManyToManyLeftEntity).WithMany(),
                _ => _.HasOne(_ => _.ManyToManyRightEntity).WithMany());
        modelBuilder.Entity<ManyToManyShadowLeftEntity>()
            .HasMany(_ => _.ManyToManyShadowRightEntities)
            .WithMany(_ => _.ManyToManyShadowLeftEntities)
            .UsingEntity("ManyToManyShadowMiddleEntity");
        modelBuilder.Entity<ReadOnlyEntity>()
            .Property(_ => _.ComputedInDb)
            .HasComputedColumnSql("Trim(Concat(Coalesce(FirstName, ''), ' ', Coalesce(LastName, '')))", stored: true);
        modelBuilder.Entity<ReadOnlyParentEntity>()
            .OrderBy(_ => _.Property);
        var fieldBuilderProjection = modelBuilder.Entity<FieldBuilderProjectionEntity>();
        fieldBuilderProjection.OrderBy(_ => _.Name);
        fieldBuilderProjection.Property(_ => _.Salary).HasPrecision(18, 2);
        modelBuilder.Entity<FieldBuilderProjectionParentEntity>().OrderBy(_ => _.Name);
        modelBuilder.Entity<DepartmentEntity>().OrderBy(_ => _.Name);
        var employeeEntity = modelBuilder.Entity<EmployeeEntity>();
        employeeEntity.OrderBy(_ => _.Name);
        employeeEntity
            .HasOne(_ => _.Department)
            .WithMany(_ => _.Employees)
            .HasForeignKey(_ => _.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure TPH inheritance for FilterBaseEntity -> FilterDerivedEntity
        modelBuilder.Entity<FilterBaseEntity>()
            .OrderBy(_ => _.CommonProperty);
        modelBuilder.Entity<FilterDerivedEntity>()
            .HasBaseType<FilterBaseEntity>();
        modelBuilder.Entity<FilterReferenceEntity>()
            .OrderBy(_ => _.Property);

        // Configure TPH inheritance with CLR discriminator property for DiscriminatorBaseEntity hierarchy
        modelBuilder.Entity<DiscriminatorBaseEntity>()
            .OrderBy(_ => _.Property);
        modelBuilder.Entity<DiscriminatorBaseEntity>()
            .HasDiscriminator(_ => _.EntityType)
            .HasValue<DiscriminatorDerivedAEntity>(DiscriminatorType.TypeA)
            .HasValue<DiscriminatorDerivedBEntity>(DiscriminatorType.TypeB)
            .IsComplete();
        modelBuilder.Entity<DiscriminatorDerivedAEntity>()
            .HasBaseType<DiscriminatorBaseEntity>();
        modelBuilder.Entity<DiscriminatorDerivedBEntity>()
            .HasBaseType<DiscriminatorBaseEntity>();

        // Configure TPH inheritance for TphRootEntity -> TphMiddleEntity -> TphLeafEntity
        modelBuilder.Entity<TphRootEntity>()
            .OrderBy(_ => _.Property);
        modelBuilder.Entity<TphMiddleEntity>()
            .HasBaseType<TphRootEntity>();
        modelBuilder.Entity<TphLeafEntity>()
            .HasBaseType<TphMiddleEntity>();
        modelBuilder.Entity<TphAttachmentEntity>()
            .OrderBy(_ => _.Property);
    }
}
