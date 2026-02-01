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

    // ReSharper disable NotAccessedPositionalProperty.Local
    record ChildFilterProjection(Guid? ParentId, string? ParentProperty, Guid ChildId);
    record AbstractNavFilterProjection(Guid? ParentId, string? ParentProperty, Guid ChildId);
}
