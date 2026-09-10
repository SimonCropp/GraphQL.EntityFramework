public partial class IntegrationTests
{
    // A field named items, edges or node was taken for a connection wrapper by its name alone,
    // so a navigation named Items was never projected and came back empty. The wrapper is now
    // recognised by the graph type the field is selected from.
    [Fact]
    public async Task Navigation_named_items_is_projected()
    {
        var query =
            """
            {
              withItems
              {
                property
                items
                {
                  property
                }
              }
            }
            """;

        var parent = new WithItemsEntity
        {
            Property = "Parent"
        };
        var item = new ItemEntity
        {
            Property = "Item",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, item]);
    }
}
