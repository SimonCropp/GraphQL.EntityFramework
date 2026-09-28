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
                  children {
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
