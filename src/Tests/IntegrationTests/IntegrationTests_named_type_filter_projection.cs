public partial class IntegrationTests
{
    /// <summary>
    /// Tests that named record types in filter projections only select required columns,
    /// not the entire navigation property.
    /// ChildEntity.Parent is a ParentEntity with only Id and Property, but in a real scenario
    /// the navigation entity would have many more columns that shouldn't be selected.
    /// </summary>
    [Fact]
    public async Task Named_record_filter_projection_should_select_minimal_columns()
    {
        var query =
            """
            {
              childEntities
              {
                property
              }
            }
            """;

        var parent1 = new ParentEntity { Property = "Parent1" };
        var parent2 = new ParentEntity { Property = "Parent2" };

        var entity1 = new ChildEntity { Property = "Child1", Parent = parent1 };
        var entity2 = new ChildEntity { Property = "Child2", Parent = parent2 };
        var entity3 = new ChildEntity { Property = "Child3", Parent = parent1 };

        var filters = new Filters<IntegrationDbContext>();

        // Use named record type with navigation property access
        // Only accesses Parent.Id and Parent.Property - should NOT include Parent in scalar fields
        filters.For<ChildEntity>().Add(
            projection: c => new ChildFilterProjection(
                c.Parent != null ? c.Parent.Id : null,
                c.Parent != null ? c.Parent.Property : null,
                c.Id),
            filter: (_, _, _, projection) => projection.ParentId == parent1.Id);

        await using var database = await sqlInstance.Build();

        // The bug causes "Parent" to be added as a scalar field, which loads the entire navigation
        // The fix should recognize navigation paths and handle them properly
        await RunQuery(database, query, null, filters, false, [parent1, parent2, entity1, entity2, entity3]);
    }

    /// <summary>
    /// Tests that filter projections accessing ABSTRACT navigation properties work correctly.
    /// When a filter projection accesses properties from an abstract navigation:
    /// 1. The navigation should be loaded via EF Include (not SELECT projection)
    /// 2. The projection should be skipped to avoid SelectExpressionBuilder failures
    /// 3. The full entity with all columns should be loaded
    /// This prevents "Can't compile a NewExpression with a constructor declared on an abstract class" errors
    /// </summary>
    [Fact]
    public async Task Filter_projection_with_abstract_navigation_should_use_include()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                property
              }
            }
            """;

        var parentId = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();

        var parent = new DerivedWithNavigationEntity
        {
            Id = parentId,
            Property = "Parent1"
        };
        var child1 = new DerivedChildEntity
        {
            Id = child1Id,
            Property = "Child1",
            TypedParent = parent
        };
        var child2 = new DerivedChildEntity
        {
            Id = child2Id,
            Property = "Child2",
            TypedParent = parent
        };

        var filters = new Filters<IntegrationDbContext>();

        // Filter projection accesses properties from abstract BaseEntity navigation
        // This should use Include instead of projection to avoid abstract type construction errors
        filters.For<DerivedChildEntity>().Add(
            projection: c => new AbstractNavFilterProjection(
                c.TypedParent != null ? c.TypedParent.Id : null,
                c.TypedParent != null ? c.TypedParent.Property : null,
                c.Id),
            filter: (_, _, _, projection) => projection.ParentId == parentId);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child1, child2]);
    }

    /// <summary>
    /// BUG TEST: Verifies that filter projections with navigation property access
    /// should only SELECT the required fields from the navigation, not all fields.
    ///
    /// This reproduces the bug where EntityFilters cause Include() to be added,
    /// which loads ALL columns from the navigation entity instead of just the
    /// filter-required columns.
    ///
    /// Expected behavior: SQL should only select Parent.Property (required by filter)
    /// and Parent.Id (primary key), not all the extra fields (Field1-Field10).
    ///
    /// Actual behavior (BUG): SQL includes ALL parent fields because Include() is used.
    /// </summary>
    [Fact]
    public async Task Filter_projection_should_not_select_all_navigation_fields()
    {
        var query =
            """
            {
              filterChildEntity(id: "{id}")
              {
                id
              }
            }
            """;

        var parent = new FilterParentEntity
        {
            Property = "AllowedParent",
            Field1 = "Extra1",
            Field2 = "Extra2",
            Field3 = "Extra3",
            Field4 = 100,
            Field5 = 200,
            Field6 = DateTime.UtcNow,
            Field7 = DateTime.UtcNow,
            Field8 = true,
            Field9 = false,
            Field10 = Guid.NewGuid()
        };
        var child = new FilterChildEntity
        {
            Property = "Child1",
            Parent = parent
        };

        query = query.Replace("{id}", child.Id.ToString());

        var filters = new Filters<IntegrationDbContext>();

        // Filter only accesses Parent.Property - should NOT load all parent fields
        filters.For<FilterChildEntity>().Add(
            projection: c => new FilterChildProjection(
                c.Parent != null ? c.Parent.Property : null,
                c.Id),
            filter: (_, _, _, projection) => projection.ParentProperty == "AllowedParent");

        await using var database = await sqlInstance.Build();

        // BUG: The SQL will include Parent.Field1, Parent.Field2, etc. even though
        // only Parent.Property is needed by the filter
        await RunQuery(database, query, null, filters, false, [parent, child]);
    }

    /// <summary>
    /// BUG TEST: Reproduces the bug where TPH inheritance + entity filters on base type
    /// cause ALL columns to be selected from the navigation entity.
    ///
    /// This matches the scenario:
    /// - FilterReferenceEntity (like Accommodation) has a navigation to FilterBaseEntity
    /// - FilterBaseEntity (like BaseRequest) is an abstract base with TPH inheritance
    /// - FilterDerivedEntity (like TravelRequest) inherits from FilterBaseEntity
    /// - Both FilterReferenceEntity and FilterBaseEntity have entity filters
    ///
    /// Expected: SQL should only select BaseEntity.CommonProperty (required by filter)
    /// Actual (BUG): SQL includes ALL fields (Field1-Field10) due to Include() being used
    /// </summary>
    [Fact]
    public async Task Filter_projection_with_TPH_inheritance_should_not_select_all_fields()
    {
        var query =
            """
            {
              filterReferenceEntity(id: "{id}")
              {
                id
              }
            }
            """;

        var derived = new FilterDerivedEntity
        {
            CommonProperty = "Allowed",
            DerivedProperty = "Derived1",
            Field1 = "Extra1",
            Field2 = "Extra2",
            Field3 = "Extra3",
            Field4 = 100,
            Field5 = 200,
            Field6 = DateTime.UtcNow,
            Field7 = DateTime.UtcNow,
            Field8 = true,
            Field9 = false,
            Field10 = Guid.NewGuid()
        };
        var reference = new FilterReferenceEntity
        {
            Property = "Reference1",
            BaseEntity = derived
        };

        query = query.Replace("{id}", reference.Id.ToString());

        var filters = new Filters<IntegrationDbContext>();

        // Filter on FilterReferenceEntity accesses BaseEntity.CommonProperty
        filters.For<FilterReferenceEntity>().Add(
            projection: r => new FilterReferenceProjection(
                r.BaseEntity != null ? r.BaseEntity.CommonProperty : null,
                r.Id),
            filter: (_, _, _, proj) => proj.BaseCommonProperty == "Allowed");

        // Filter on base type (like BaseRequest in MinistersApi)
        filters.For<FilterBaseEntity>().Add(
            projection: b => new FilterBaseProjection(b.CommonProperty, b.Id),
            filter: (_, _, _, proj) => proj.CommonProperty != null);

        await using var database = await sqlInstance.Build();

        // BUG: The SQL will include a subquery selecting ALL fields from FilterBaseEntity
        // (Field1-Field10) even though only CommonProperty is needed
        await RunQuery(database, query, null, filters, false, [derived, reference]);
    }

    // ReSharper disable NotAccessedPositionalProperty.Local
    record ChildFilterProjection(Guid? ParentId, string? ParentProperty, Guid ChildId);
    record AbstractNavFilterProjection(Guid? ParentId, string? ParentProperty, Guid ChildId);
    record FilterChildProjection(string? ParentProperty, Guid ChildId);
    record FilterReferenceProjection(string? BaseCommonProperty, Guid Id);
    record FilterBaseProjection(string? CommonProperty, Guid Id);
}
