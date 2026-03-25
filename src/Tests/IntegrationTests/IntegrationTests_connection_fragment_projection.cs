public partial class IntegrationTests
{
    [Fact]
    public async Task Connection_items_with_inline_fields()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                items {
                  property
                }
              }
            }
            """;
        var entities = BuildEntities(8);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    [Fact]
    public async Task Connection_items_with_fragment_spread()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                items {
                  ...parentFields
                }
              }
            }

            fragment parentFields on Parent {
              property
            }
            """;
        var entities = BuildEntities(8);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    [Fact]
    public async Task Connection_edges_node_with_fragment_spread()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                edges {
                  cursor
                  node {
                    ...parentFields
                  }
                }
              }
            }

            fragment parentFields on Parent {
              property
            }
            """;
        var entities = BuildEntities(8);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    [Fact]
    public async Task Connection_items_with_inline_fragment()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                items {
                  ... on Parent {
                    property
                  }
                }
              }
            }
            """;
        var entities = BuildEntities(8);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    [Fact]
    public async Task Connection_items_with_fragment_spread_and_children()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                items {
                  ...parentWithChildren
                }
              }
            }

            fragment parentWithChildren on Parent {
              property
              children {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2"
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3"
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5"
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }
}
