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

    record ChildFilterProjection(Guid? ParentId, string? ParentProperty, Guid ChildId);
}
