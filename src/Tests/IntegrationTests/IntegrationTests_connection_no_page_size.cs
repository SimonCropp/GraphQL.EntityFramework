public partial class IntegrationTests
{
    // With neither first nor last, and no page size on the field, first defaulted to zero and the
    // connection came back with the total count but no edges. It now returns everything.
    [Fact]
    public async Task Connection_without_first_or_last_returns_everything()
    {
        var query =
            """
            {
              childEntitiesConnection(orderBy: property)
              {
                totalCount
                pageInfo
                {
                  hasNextPage
                  hasPreviousPage
                  startCursor
                  endCursor
                }
                edges
                {
                  cursor
                  node
                  {
                    property
                  }
                }
              }
            }
            """;

        var child1 = new ChildEntity
        {
            Property = "Value1"
        };
        var child2 = new ChildEntity
        {
            Property = "Value2"
        };
        var child3 = new ChildEntity
        {
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [child1, child2, child3]);
    }
}
