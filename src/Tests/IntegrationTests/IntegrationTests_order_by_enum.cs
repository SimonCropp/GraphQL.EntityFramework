public partial class IntegrationTests
{
    [Fact]
    public async Task OrderByEnum_schema()
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
            orderByStyle: OrderByStyle.Enum);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        schema.Initialize();

        // Child and Parent for the common shape, Level1 for the nesting cap, and
        // IncludeNonQueryableA since it and B navigate to each other
        string[] names =
        [
            "ChildEntityOrderBy",
            "ParentEntityOrderBy",
            "Level1EntityOrderBy",
            "IncludeNonQueryableAOrderBy"
        ];
        var enums = schema.AllTypes
            .OfType<EnumerationGraphType>()
            .Where(_ => names.Contains(_.Name))
            .OrderBy(_ => _.Name, StringComparer.Ordinal)
            .Select(_ => $"{_.Name}: {string.Join(", ", _.Values.Select(value => value.Name))}");
        await Verify(string.Join(Environment.NewLine, enums))
            .Snapshot(
                """
                ChildEntityOrderBy: id, id_desc, nullable, nullable_desc, parent_id, parent_id_desc, parent_property, parent_property_desc, parentId, parentId_desc, property, property_desc
                IncludeNonQueryableAOrderBy: id, id_desc, includeNonQueryableB_id, includeNonQueryableB_id_desc, includeNonQueryableB_includeNonQueryableAId, includeNonQueryableB_includeNonQueryableAId_desc, includeNonQueryableBId, includeNonQueryableBId_desc
                Level1EntityOrderBy: id, id_desc, level2Entity_id, level2Entity_id_desc, level2Entity_level3EntityId, level2Entity_level3EntityId_desc, level2EntityId, level2EntityId_desc
                ParentEntityOrderBy: id, id_desc, property, property_desc
                """);
    }

    [Fact]
    public async Task OrderByEnum_single()
    {
        var query =
            """
            {
              childEntities (orderBy: property)
              {
                property
              }
            }
            """;

        await RunOrderByEnumQuery(query);
    }

    [Fact]
    public async Task OrderByEnum_descending()
    {
        var query =
            """
            {
              childEntities (orderBy: property_desc)
              {
                property
              }
            }
            """;

        await RunOrderByEnumQuery(query);
    }

    [Fact]
    public async Task OrderByEnum_two_keys()
    {
        var query =
            """
            {
              childEntities (orderBy: [nullable_desc, property])
              {
                property
                nullable
              }
            }
            """;

        await RunOrderByEnumQuery(query);
    }

    [Fact]
    public async Task OrderByEnum_nested()
    {
        var query =
            """
            {
              childEntities (orderBy: [parent_property_desc, property])
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        await RunOrderByEnumQuery(query);
    }

    [Fact]
    public async Task OrderByEnum_with_typed_variable()
    {
        var query =
            """
            query ($orderBy: [ChildEntityOrderBy!])
            {
              childEntities (orderBy: $orderBy)
              {
                property
              }
            }
            """;

        var inputs = new Inputs(
            new Dictionary<string, object?>
            {
                {
                    "orderBy", new List<object?>
                    {
                        "property_desc"
                    }
                }
            });
        await RunOrderByEnumQuery(query, inputs);
    }

    [Fact]
    public async Task OrderByEnum_unknown_value()
    {
        var query =
            """
            {
              childEntities (orderBy: notAProperty)
              {
                property
              }
            }
            """;

        await RunOrderByEnumQuery(query);
    }

    static Task RunOrderByEnumQuery(string query, Inputs? inputs = null, [CallerFilePath] string sourceFile = "")
    {
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
            Property = "A",
            Nullable = 1,
            Parent = parent1
        };
        var child2 = new ChildEntity
        {
            Property = "B",
            Nullable = 2,
            Parent = parent2
        };
        var child3 = new ChildEntity
        {
            Property = "C",
            Nullable = 1,
            Parent = parent1
        };

        return RunQuery(query, [parent1, parent2, child1, child2, child3], inputs, sourceFile);
    }

    static async Task RunQuery(string query, object[] entities, Inputs? inputs, string sourceFile)
    {
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, entities, orderByStyle: OrderByStyle.Enum, sourceFile: sourceFile);
    }
}
