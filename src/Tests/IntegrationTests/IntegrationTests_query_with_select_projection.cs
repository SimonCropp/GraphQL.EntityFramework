public partial class IntegrationTests
{
    /// <summary>
    /// Tests that queries with Select projections in the resolver work correctly with filters.
    /// This scenario occurs when a resolver uses .Select() to project data from a view or
    /// navigation property. In this case, Include cannot be added to the query because
    /// EF Core throws "Include has been used on non entity queryable" when Include is
    /// applied after Select.
    /// </summary>
    [Fact]
    public async Task Query_with_select_projection_and_filter()
    {
        var query =
            """
            {
              queryFieldWithInclude {
                id
              }
            }
            """;

        var entityA = new IncludeNonQueryableA();
        var entityB = new IncludeNonQueryableB
        {
            IncludeNonQueryableA = entityA,
            IncludeNonQueryableAId = entityA.Id
        };
        entityA.IncludeNonQueryableB = entityB;
        entityA.IncludeNonQueryableBId = entityB.Id;

        // Add a filter to IncludeNonQueryableA - this entity is returned via .Select() projection
        // The filter should work without attempting to add Include to the projected query
        var filters = new Filters<IntegrationDbContext>();
        filters.For<IncludeNonQueryableA>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entityA, entityB]);
    }

    /// <summary>
    /// Tests that queries with Select projections work when the filter accesses a navigation property.
    /// This is the scenario from MinistersApi where GroundTransportOrderedView.Select(_ => _.GroundTransport)
    /// is used, and the filter accesses GroundTransport.TravelRequest.HighestStatusAchieved.
    /// </summary>
    [Fact]
    public async Task Query_with_select_projection_and_filter_accessing_navigation()
    {
        var query =
            """
            {
              queryFieldWithInclude {
                id
              }
            }
            """;

        var entityA = new IncludeNonQueryableA();
        var entityB = new IncludeNonQueryableB
        {
            IncludeNonQueryableA = entityA,
            IncludeNonQueryableAId = entityA.Id
        };
        entityA.IncludeNonQueryableB = entityB;
        entityA.IncludeNonQueryableBId = entityB.Id;

        // Add a filter that accesses a navigation property
        // This would normally trigger Include for the navigation, but since the query
        // already has Select applied, Include cannot be added
        var filters = new Filters<IntegrationDbContext>();
        filters.For<IncludeNonQueryableA>().Add(
            projection: _ => new
            {
                _.Id,
                ParentId = _.IncludeNonQueryableB.Id
            },
            filter: (_, _, _, data) => data.Id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entityA, entityB]);
    }

    /// <summary>
    /// Tests that regular queries (without Select projection in resolver) still work
    /// correctly with filters that require navigation properties.
    /// </summary>
    [Fact]
    public async Task Query_without_select_projection_with_navigation_filter()
    {
        var query =
            """
            {
              childEntities {
                id
                property
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent1"
        };
        var child = new ChildEntity
        {
            Property = "Child1",
            Parent = parent
        };
        parent.Children.Add(child);

        // Add a filter that accesses the parent navigation
        var filters = new Filters<IntegrationDbContext>();
        filters.For<ChildEntity>().Add(
            projection: _ => new
            {
                _.Id,
                ParentProperty = _.Parent != null ? _.Parent.Property : null
            },
            filter: (_, _, _, data) => data.ParentProperty != null);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child]);
    }
}
