// OrderByStyle.Object is not the default, so every query here opts in
public partial class IntegrationTests
{
    [Fact]
    public async Task OrderByObject_schema()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton<Mutation>();
        services.AddSingleton(database.Context);
        services.AddGraphQL(null);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, _) => dbContext,
            dbContext.Model,
            orderByStyle: OrderByStyle.Object);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        schema.Initialize();

        // The same types the enum style prints in OrderByEnum_schema. A reference navigation
        // nests into another input type rather than being flattened, so there is no depth cap
        string[] names =
        [
            "ChildEntityOrderBy",
            "ParentEntityOrderBy",
            "Level1EntityOrderBy",
            "IncludeNonQueryableAOrderBy"
        ];
        var inputs = schema.AllTypes
            .OfType<IInputObjectGraphType>()
            .Where(_ => names.Contains(_.Name))
            .OrderBy(_ => _.Name, StringComparer.Ordinal)
            .Select(_ => $"{_.Name}: {string.Join(", ", _.Fields.Select(field => $"{field.Name}: {field.ResolvedType!.Name}"))}");
        await Verify(string.Join(Environment.NewLine, inputs))
            .Snapshot(
                """
                ChildEntityOrderBy: id: SortDirection, nullable: SortDirection, parent: ParentEntityOrderBy, parentId: SortDirection, property: SortDirection
                IncludeNonQueryableAOrderBy: id: SortDirection, includeNonQueryableB: IncludeNonQueryableBOrderBy, includeNonQueryableBId: SortDirection
                Level1EntityOrderBy: id: SortDirection, level2Entity: Level2EntityOrderBy, level2EntityId: SortDirection
                ParentEntityOrderBy: id: SortDirection, property: SortDirection
                """);
    }

    [Fact]
    public async Task OrderByObject_single()
    {
        var query =
            """
            {
              childEntities (orderBy: {property: ascending})
              {
                property
              }
            }
            """;

        await RunOrderByObjectQuery(query);
    }

    [Fact]
    public async Task OrderByObject_two_keys()
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
        await RunQuery(database, query, null, null, false, [parent, child1, child2, child3], orderByStyle: OrderByStyle.Object);
    }

    [Fact]
    public async Task OrderByObject_nested_descending()
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
        await RunQuery(database, query, null, null, false, [parent1, parent2, child1, child2, child3], orderByStyle: OrderByStyle.Object);
    }

    [Fact]
    public async Task OrderByObject_item_with_two_properties()
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
        await RunQuery(database, query, null, null, false, [], orderByStyle: OrderByStyle.Object);
    }

    [Fact]
    public async Task OrderByObject_empty_item()
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
        await RunQuery(database, query, null, null, false, [], orderByStyle: OrderByStyle.Object);
    }

    [Fact]
    public async Task OrderByObject_with_typed_variable()
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
        await RunQuery(database, query, inputs, null, false, [entity1, entity2], orderByStyle: OrderByStyle.Object);
    }

    [Fact]
    public async Task OrderByObject_unknown_property()
    {
        var query =
            """
            {
              childEntities (orderBy: {notAProperty: ascending})
              {
                property
              }
            }
            """;

        await RunOrderByObjectQuery(query);
    }

    static async Task RunOrderByObjectQuery(string query, [CallerFilePath] string sourceFile = "")
    {
        var parent = new ParentEntity
        {
            Property = "Parent1"
        };
        var child1 = new ChildEntity
        {
            Property = "A",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Property = "B",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2], orderByStyle: OrderByStyle.Object, sourceFile: sourceFile);
    }
}
