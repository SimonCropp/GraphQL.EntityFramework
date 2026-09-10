public partial class IntegrationTests
{
    [Fact]
    public async Task OrderBy_two_keys()
    {
        var query =
            """
            {
              childEntities (orderBy: [{nullable: descending}, {property: ascending}])
              {
                property
                nullable
              }
            }
            """;

        var parent = new ParentEntity();
        var child1 = new ChildEntity
        {
            Property = "B",
            Nullable = 1,
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Property = "A",
            Nullable = 1,
            Parent = parent
        };
        var child3 = new ChildEntity
        {
            Property = "C",
            Nullable = 2,
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2, child3]);
    }

    [Fact]
    public async Task OrderBy_nested_descending()
    {
        var query =
            """
            {
              childEntities (orderBy: [{parent: {property: descending}}, {property: ascending}])
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var parent1 = new ParentEntity
        {
            Property = "Parent1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Parent2"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = parent2
        };
        var child3 = new ChildEntity
        {
            Property = "Child3",
            Parent = parent2
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent1, parent2, child1, child2, child3]);
    }

    [Fact]
    public async Task OrderBy_item_with_two_properties()
    {
        var query =
            """
            {
              parentEntities (orderBy: {property: ascending, id: ascending})
              {
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task OrderBy_empty_item()
    {
        var query =
            """
            {
              parentEntities (orderBy: {})
              {
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task OrderBy_with_typed_variable()
    {
        var query =
            """
            query ($orderBy: [ParentEntityOrderBy!])
            {
              parentEntities (orderBy: $orderBy)
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        var inputs = new Inputs(new Dictionary<string, object?>
        {
            {
                "orderBy", new List<object?>
                {
                    new Dictionary<string, object?>
                    {
                        {
                            "property", "descending"
                        }
                    }
                }
            }
        });
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, [entity1, entity2]);
    }
}
