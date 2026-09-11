public partial class IntegrationTests
{
    // The where of a navigation list was applied in memory to the projected children, so it
    // followed the ordinal in memory rules rather than the database collation, and it only saw
    // the columns the selection had asked for. It is now applied inside the collection subquery.
    [Fact]
    public async Task Navigation_where_evaluates_in_sql()
    {
        var query =
            """
            {
              parentEntities
              {
                children(where: {property: {equal: "VALUE1"}})
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Navigation_where_on_unselected_property()
    {
        var query =
            """
            {
              parentEntities
              {
                children(where: {property: {equal: "Value1"}})
                {
                  id
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Navigation_orderBy_on_unselected_property()
    {
        var query =
            """
            {
              parentEntities
              {
                children(orderBy: property_desc)
                {
                  nullable
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "A",
            Nullable = 1,
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "B",
            Nullable = 2,
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Navigation_ids_evaluates_in_sql()
    {
        var query =
            """
            {
              parentEntities
              {
                children(ids: "00000000-0000-0000-0000-000000000002")
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Navigation_where_from_variable()
    {
        var query =
            """
            query($value: String!)
            {
              parentEntities
              {
                children(where: {property: {equal: $value}})
                {
                  id
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        var inputs = new Inputs(new Dictionary<string, object?>
        {
            ["value"] = "Value2"
        });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, [parent, child1, child2]);
    }

    // Skip and take stay with the resolver, applied to the collection the query filtered and ordered
    [Fact]
    public async Task Navigation_skip_take_after_sql_where()
    {
        var query =
            """
            {
              parentEntities
              {
                children(where: {property: {startsWith: "Value"}}, orderBy: property, skip: 1, take: 1)
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };
        var child3 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Other",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2, child3]);
    }

    // One loaded collection cannot satisfy two sets of arguments, so aliases with different
    // arguments fall back to loading the collection whole and filtering in memory
    [Fact]
    public async Task Navigation_aliases_with_different_where()
    {
        var query =
            """
            {
              parentEntities
              {
                first: children(where: {property: {equal: "Value1"}})
                {
                  property
                }
                second: children(where: {property: {equal: "VALUE2"}})
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Navigation_connection_where_evaluates_in_sql()
    {
        var query =
            """
            {
              parentEntities
              {
                childrenConnection(where: {property: {equal: "VALUE1"}}, first: 5)
                {
                  totalCount
                  items
                  {
                    property
                  }
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    // A resolver that returns something other than the projected collection keeps the in memory
    // pass, which now ignores case the way the database does
    [Fact]
    public async Task Navigation_custom_resolver_where_in_memory()
    {
        var query =
            """
            {
              parentEntities
              {
                childrenReversed(where: {property: {equal: "VALUE1"}})
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }
}
