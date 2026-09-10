public partial class IntegrationTests
{
    // Below the root, the edges/items/node wrappers of a connection were recorded as scalar fields
    // named after the wrapper, so only the keys of the nodes were projected and every other
    // scalar came back null.
    [Fact]
    public async Task Nested_connection_projects_scalars()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                childrenConnection(first: 10)
                {
                  totalCount
                  items
                  {
                    property
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
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        // Fixed ids, since the nested collection is ordered by key
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value2",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value3",
            Parent = parent
        };
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Nested_connection_under_root_connection_projects_scalars()
    {
        var query =
            """
            {
              parentEntitiesConnection(first: 10)
              {
                items
                {
                  property
                  childrenConnection(first: 10)
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

        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        var child = new ChildEntity
        {
            Property = "Value2",
            Parent = parent
        };
        parent.Children.Add(child);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    // Projection metadata was only looked up for the root sub fields. A projection based field
    // below a navigation was matched against the entity by its GraphQL name, found nothing, and
    // so the navigation it projects was never loaded.
    [Fact]
    public async Task Nested_projection_based_navigation_field()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                children
                {
                  property
                  parentAlias
                  {
                    property
                  }
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        var child = new ChildEntity
        {
            Property = "Value2",
            Parent = parent
        };
        parent.Children.Add(child);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    [Fact]
    public async Task Nested_projection_based_scalar_fields()
    {
        var query =
            """
            {
              fieldBuilderProjectionParents
              {
                name
                children(first: 10)
                {
                  items
                  {
                    name
                    statusDisplay
                    statusViaWithProjection
                  }
                }
              }
            }
            """;

        var parent = new FieldBuilderProjectionParentEntity
        {
            Name = "TheParent"
        };
        var child = new FieldBuilderProjectionEntity
        {
            Name = "TheChild",
            Status = EntityStatus.Pending,
            Parent = parent
        };
        parent.Children.Add(child);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }
}
