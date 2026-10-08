public partial class IntegrationTests
{
    // A navigation to an abstract type is projected as a chain of type tests, and the collection
    // under it is bound once to the result of that chain. The navigation used to be bound whole,
    // with what was requested under it loaded through includes applied alongside the select.
    [Fact]
    public async Task Navigation_under_abstract_navigation_is_loaded()
    {
        var query =
            """
            {
              derivedChildEntities(where: {property: {equal: "Child1"}})
              {
                property
                parent
                {
                  property
                  childrenFromInterface(orderBy: {property: ascending})
                  {
                    items
                    {
                      property
                    }
                  }
                }
              }
            }
            """;

        var parent = new DerivedEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Parent"
        };
        var child1 = new DerivedChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Child1",
            Parent = parent
        };
        var child2 = new DerivedChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Child2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }
}
