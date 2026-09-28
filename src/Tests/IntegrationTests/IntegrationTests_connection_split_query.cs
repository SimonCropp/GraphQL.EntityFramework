public partial class IntegrationTests
{
    // The resolver's AsSplitQuery is removed, since no collection is selected
    [Fact]
    public async Task Connection_split_query_without_collection()
    {
        var query =
            """
            {
              splitParentEntitiesConnection(first: 10) {
                totalCount
                items {
                  property
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }

    // The resolver's AsSplitQuery is removed from a connection over a type with derived types,
    // since no collection is selected
    [Fact]
    public async Task Connection_split_query_with_derived_types_without_collection()
    {
        var query =
            """
            {
              splitInterfaceGraphConnection(first: 10) {
                totalCount
                items {
                  property
                }
              }
            }
            """;

        var derived1 = new DerivedEntity
        {
            Property = "Value1"
        };
        var derived2 = new DerivedWithNavigationEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [derived1, derived2]);
    }

    // The resolver's AsSplitQuery is kept, since the children are selected
    [Fact]
    public async Task Connection_split_query_with_collection()
    {
        var query =
            """
            {
              splitParentEntitiesConnection(first: 10) {
                totalCount
                items {
                  property
                  children(orderBy: {property: ascending}) {
                    property
                  }
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }
}
