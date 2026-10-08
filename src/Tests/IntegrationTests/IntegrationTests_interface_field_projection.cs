public partial class IntegrationTests
{
    // statusSummary is declared on the interface with no projection, and on each implementing type
    // with a resolver that reads Status and a projection for it. Status defaults to Draft, so the
    // summary only says Approved when the projection of the implementing type's field was loaded.
    // Selected on the interface it was not: the field of the interface carries no projection, and
    // only that field was looked at.

    [Fact]
    public async Task Interface_field_uses_projection_of_implementing_types()
    {
        var query =
            """
            {
              baseEntities(orderBy: {property: ascending})
              {
                property
                statusSummary
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, InterfaceProjectionEntities());
    }

    // A fragment at the root of an interface typed query is not among the sub fields, so its
    // fields were recorded by name alone and their projections never read.
    [Fact]
    public async Task Interface_field_in_root_fragment_uses_its_projection()
    {
        var query =
            """
            {
              baseEntities(orderBy: {property: ascending})
              {
                property
                ... on Derived
                {
                  statusSummary
                }
                ... on DerivedWithNavigation
                {
                  statusSummary
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, InterfaceProjectionEntities());
    }

    [Fact]
    public async Task Interface_field_in_connection_uses_projection_of_implementing_types()
    {
        var query =
            """
            {
              interfaceGraphConnection
              {
                items
                {
                  property
                  statusSummary
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, InterfaceProjectionEntities());
    }

    [Fact]
    public async Task Interface_field_under_navigation_uses_projection_of_implementing_types()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                property
                parent
                {
                  statusSummary
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, InterfaceProjectionEntities());
    }

    static object[] InterfaceProjectionEntities()
    {
        var derived = new DerivedEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Derived",
            Status = "Approved"
        };
        var withNavigation = new DerivedWithNavigationEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "WithNavigation",
            Status = "Approved"
        };
        var child = new DerivedChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Child",
            Parent = derived
        };
        return [derived, withNavigation, child];
    }
}
