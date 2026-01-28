public partial class IntegrationTests
{
    /// <summary>
    /// This test reproduces the bug where a filter with projection is defined on a BASE type,
    /// but the GraphQL query is for a DERIVED type in a TPH hierarchy.
    ///
    /// Scenario (from MinistersApi):
    /// - Filter defined on BaseRequest with projection: _ => new { _.GroupOwnerId, _.HighestStatusAchieved, _.Id }
    /// - GraphQL query for ministerTravelRequest (which is MinisterTravelRequest : TravelRequest : BaseRequest)
    /// - Bug: HighestStatusAchieved is not included in the SQL projection
    /// - Result: Filter receives default value (Draft) instead of actual value (e.g., AdviceRequested)
    /// - This causes the filter to fail incorrectly
    ///
    /// Expected: When querying a derived type, projections from base type filters should be included
    /// </summary>
    [Fact]
    public async Task Query_derived_type_with_filter_projection_on_base_type()
    {
        // Query for derivedEntity (specific derived type)
        var query =
            """
            {
              derivedEntity(id: "00000000-0000-0000-0000-000000000001") {
                id
                property
              }
            }
            """;

        var derived = new DerivedEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "DerivedProp",
            Status = "AdviceRequested" // Set to non-default value
        };

        // Define filter with projection on BASE type (BaseEntity)
        // but query is for DERIVED type (DerivedEntity)
        var filters = new Filters<IntegrationDbContext>();
        filters.For<BaseEntity>().Add(
            projection: _ => new
            {
                _.Id,
                _.Status  // This should be included in SQL projection even though query is for DerivedEntity
            },
            filter: (_, _, _, data) =>
            {
                // This filter should receive Status = "AdviceRequested"
                // Bug: It receives Status = "Draft" (default value) because Status isn't projected
                return data.Status == "AdviceRequested";
            });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derived]);
    }

    /// <summary>
    /// Similar test but with a collection query instead of single entity query
    /// </summary>
    [Fact]
    public async Task Query_derived_collection_with_filter_projection_on_base_type()
    {
        var query =
            """
            {
              derivedEntities {
                id
                property
              }
            }
            """;

        var derived1 = new DerivedEntity
        {
            Property = "Derived1",
            Status = "AdviceRequested"
        };
        var derived2 = new DerivedEntity
        {
            Property = "Derived2",
            Status = "Draft"
        };

        // Filter on base type with projection
        var filters = new Filters<IntegrationDbContext>();
        filters.For<BaseEntity>().Add(
            projection: _ => _.Status,
            filter: (_, _, _, status) => status == "AdviceRequested");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derived1, derived2]);
    }

    /// <summary>
    /// Test with multiple derived types in the same hierarchy
    /// Filter defined on base should work for all derived types
    /// </summary>
    [Fact]
    public async Task Query_multiple_derived_types_with_base_type_filter()
    {
        var query =
            """
            {
              baseEntities {
                property
              }
            }
            """;

        var derived1 = new DerivedEntity
        {
            Property = "Derived1",
            Status = "AdviceRequested"
        };
        var derived2 = new DerivedWithNavigationEntity
        {
            Property = "Derived2",
            Status = "Draft"
        };
        var derived3 = new DerivedEntity
        {
            Property = "Derived3",
            Status = "AdviceRequested"
        };

        // Filter on base type should apply to all derived types
        var filters = new Filters<IntegrationDbContext>();
        filters.For<BaseEntity>().Add(
            projection: _ => new { _.Id, _.Status },
            filter: (_, _, _, data) => data.Status == "AdviceRequested");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derived1, derived2, derived3]);
    }
}
