public partial class IntegrationTests
{
    // An abstract navigation type cannot be projected, so the navigation is bound whole. The
    // navigations requested under it were then never loaded, since nothing added an include for
    // them. They now arrive through includes applied alongside the select.
    [Fact]
    public async Task Navigation_under_abstract_navigation_is_loaded()
    {
        var query =
            """
            {
              derivedChildEntities(where: {path: "property", comparison: equal, value: "Child1"})
              {
                property
                parent
                {
                  property
                  childrenFromInterface(orderBy: {path: "property"})
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
