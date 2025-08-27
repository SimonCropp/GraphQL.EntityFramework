public partial class IntegrationTests
{
    static SqlInstance<IntegrationDbContext> sqlInstance;

    static IntegrationTests() =>
        sqlInstance = new(
            buildTemplate: async data =>
            {
                var database = data.Database;
                await database.EnsureCreatedAsync();
                await database.ExecuteSqlRawAsync(
                    """
                    create view ParentEntityView as
                            select Property
                            from ParentEntities
                    """);
            },
            constructInstance: builder =>
            {
                builder.ConfigureWarnings(_ =>
                    _.Ignore(
                        CoreEventId.NavigationBaseIncludeIgnored,
                        CoreEventId.ShadowForeignKeyPropertyCreated,
                        CoreEventId.CollectionWithoutComparer));
                return new(builder.Options);
            });

    [Fact]
    public async Task SchemaPrint()
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

        EfGraphQLConventions.RegisterInContainer(services, (_, _) => dbContext, dbContext.Model);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);

        var print = schema.Print();
        await Verify(print);
    }

    [Fact]
    public async Task LogQuery()
    {
        string? queryText = null;
        QueryLogger.Enable(s => queryText = s);
        var query =
            """
            {
              parentEntities
              (where:
                [
                  {path: "Property", comparison: startsWith, value: "Valu"}
                  {path: "Property", comparison: endsWith, value: "ue3"}
                ]
              )
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
        var entity3 = new ParentEntity
        {
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
        Assert.NotNull(queryText);
    }

    [Fact]
    public async Task Where_multiple()
    {
        var query =
            """
            {
              parentEntities
              (where:
                [
                  {path: "Property", comparison: startsWith, value: "Valu"}
                  {path: "Property", comparison: endsWith, value: "ue3"}
                ]
              )
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
        var entity3 = new ParentEntity
        {
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Where_date()
    {
        var query =
            """
            {
              dateEntities (where: {path: "Property", comparison: equal, value: "2020-10-1"})
              {
                id
              }
            }
            """;

        var entity1 = new DateEntity();
        var entity2 = new DateEntity
        {
            Property = new Date(2020, 10, 1)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_date_notEqual()
    {
        var query =
            """
            {
              dateEntities (where: {path: "Property", comparison: notEqual, value: "2020-10-1"})
              {
                id
              }
            }
            """;

        var entity1 = new DateEntity();
        var entity2 = new DateEntity
        {
            Property = new Date(2020, 10, 1)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_enum()
    {
        var query =
            """
            {
              enumEntities (where: {path: "Property", comparison: equal, value: "Thursday"})
              {
                id
              }
            }
            """;

        var entity1 = new EnumEntity();
        var entity2 = new EnumEntity
        {
            Property = DayOfWeek.Thursday
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_enum_notEqual()
    {
        var query =
            """
            {
              enumEntities (where: {path: "Property", comparison: notEqual, value: "Thursday"})
              {
                id
              }
            }
            """;

        var entity1 = new EnumEntity();
        var entity2 = new EnumEntity
        {
            Property = DayOfWeek.Thursday
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_string_notEqual()
    {
        var query =
            """
            {
              stringEntities (orderBy: {path: "property"}, where: {path: "Property", comparison: notEqual, value: "notValue"})
              {
                id, property
              }
            }
            """;

        var entity1 = new StringEntity();
        var entity2 = new StringEntity
        {
            Property = "Value"
        };
        var entity3 = new StringEntity
        {
            Property = "notValue"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Where_string_notEqual_diffCase()
    {
        var query =
            """
            {
              stringEntities (orderBy: {path: "property"}, where: {path: "Property", comparison: notEqual, value: "NotValue"})
              {
                id, property
              }
            }
            """;

        var entity1 = new StringEntity();
        var entity2 = new StringEntity
        {
            Property = "Value"
        };
        var entity3 = new StringEntity
        {
            Property = "notValue"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Where_string_contains()
    {
        var query =
            """
            {
              stringEntities (orderBy: {path: "property"}, where: {path: "Property", comparison: contains, value: "b" })
              {
                id, property
              }
            }
            """;

        var entity1 = new StringEntity();
        var entity2 = new StringEntity
        {
            Property = "a"
        };
        var entity3 = new StringEntity
        {
            Property = "ab"
        };
        var entity4 = new StringEntity
        {
            Property = "abc"
        };
        var entity5 = new StringEntity
        {
            Property = "bc"
        };
        var entity6 = new StringEntity
        {
            Property = "c"
        };
        var entity7 = new StringEntity
        {
            Property = "b"
        };
        var entity8 = new StringEntity
        {
            Property = ""
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5, entity6, entity7, entity8]);
    }

    [Fact]
    public async Task Where_string_contains_diffCase()
    {
        var query =
            """
            {
              stringEntities (orderBy: {path: "property"}, where: {path: "Property", comparison: contains, value: "B" })
              {
                id, property
              }
            }
            """;

        var entity1 = new StringEntity();
        var entity2 = new StringEntity
        {
            Property = "a"
        };
        var entity3 = new StringEntity
        {
            Property = "ab"
        };
        var entity4 = new StringEntity
        {
            Property = "abc"
        };
        var entity5 = new StringEntity
        {
            Property = "bc"
        };
        var entity6 = new StringEntity
        {
            Property = "c"
        };
        var entity7 = new StringEntity
        {
            Property = "b"
        };
        var entity8 = new StringEntity
        {
            Property = ""
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5, entity6, entity7, entity8]);
    }


    [Fact]
    public async Task Where_enum_in()
    {
        var query =
            """
            {
              enumEntities (where: {path: "Property", comparison: in, value: ["Thursday"]})
              {
                id
              }
            }
            """;

        var entity1 = new EnumEntity();
        var entity2 = new EnumEntity
        {
            Property = DayOfWeek.Thursday
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_time()
    {
        var query =
            """
            {
              timeEntities (where: {path: "Property", comparison: equal, value: "10:11 AM"})
              {
                id
              }
            }
            """;

        var entity1 = new TimeEntity();
        var entity2 = new TimeEntity
        {
            Property = new Time(10, 11)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_time_notEqual()
    {
        var query =
            """
            {
              timeEntities (where: {path: "Property", comparison: notEqual, value: "10:11 AM"})
              {
                id
              }
            }
            """;

        var entity1 = new TimeEntity();
        var entity2 = new TimeEntity
        {
            Property = new Time(10, 11)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_with_nullable_properties1()
    {
        var query =
            """{ withNullableEntities (where: {path: "Nullable", comparison: equal}){ id } }""";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity

        {
            Nullable = 10
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_with_nullable_properties1_NotEqual()
    {
        var query =
            """{ withNullableEntities (where: {path: "Nullable", comparison: notEqual}){ id } }""";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity

        {
            Nullable = 10
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_with_nullable_properties2()
    {
        var query =
            """{ withNullableEntities (where: {path: "Nullable", comparison: equal, value: "10"}){ id } }""";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity
        {
            Nullable = 10
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_with_nullable_properties2_notEqual()
    {
        var query =
            """{ withNullableEntities (where: {path: "Nullable", comparison: notEqual, value: "10"}){ id } }""";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity
        {
            Nullable = 10
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_null_comparison_value()
    {
        var query =
            """{ parentEntities (where: {path: "Property", comparison: equal}){ id } }""";

        var entity1 = new ParentEntity
        {
            Property = null
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_null_comparison_value_notEqual()
    {
        var query =
            """{ parentEntities (where: {path: "Property", comparison: notEqual}){ id } }""";

        var entity1 = new ParentEntity
        {
            Property = null
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Take()
    {
        var query =
            """
            {
              parentEntities (take: 1, orderBy: {path: "property"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task TakeNoOrder()
    {
        var query =
            """
            {
              parentEntities (take: 1)
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Skip()
    {
        var query =
            """
            {
              parentEntities (skip: 1, orderBy: {path: "property"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task SkipNoOrder()
    {
        var query =
            """
            {
              parentEntities (skip: 1)
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Connection_first_page()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                edges {
                  cursor
                  node {
                    property
                  }
                }
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
    public async Task Connection_page_back()
    {
        var query =
            """
            {
              parentEntitiesConnection(last:2, before: "2") {
                totalCount
                edges {
                  cursor
                  node {
                    property
                  }
                }
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
    public async Task Connection_nested()
    {
        var query =
            """
            {
              parentEntities {
                id
                childrenConnection(first:2, after:"2") {
                  edges {
                    cursor
                    node {
                      id
                    }
                  }
                  pageInfo {
                      endCursor
                      hasNextPage
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
    public async Task Connection_nested_OmitQueryArguments()
    {
        var query =
            """
            {
              parentEntities {
                id
                childrenConnectionOmitQueryArguments {
                  edges {
                    cursor
                    node {
                      id
                    }
                  }
                  pageInfo {
                      endCursor
                      hasNextPage
                    }
                }
              }
            }
            """;
        var entities = BuildEntities(8);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    static IEnumerable<ParentEntity> BuildEntities(uint length)
    {
        for (var index = 0; index < length; index++)
        {
            yield return new()
            {
                Id = new("00000000-0000-0000-0000-00000000000" + index),
                Property = "Value" + index
            };
        }
    }

    [Fact]
    public async Task OrderBy()
    {
        var query =
            """
            {
              parentEntities (orderBy: {path: "Property"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity2, entity1]);
    }

    [Fact]
    public async Task OrderByDescending()
    {
        var query =
            """
            {
              parentEntities (orderBy: {path: "Property", descending: true})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task OrderByNullable()
    {
        var query =
            """
            {
              parentEntities (orderBy: {path: "Property"})
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ParentEntity();

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task OrderByNested()
    {
        var query =
            """
            {
              childEntities (orderBy: {path: "Parent.Property"})
              {
                property
              }
            }
            """;

        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity1 = new ChildEntity
        {
            Property = "Value2",
            Parent = parent1
        };
        var parent2 = new ParentEntity
        {
            Property = "Value3"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value4",
            Parent = parent2
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent1, parent2, entity1, entity2]);
    }

    [Fact]
    public async Task OrderByNestedNullable()
    {
        var query =
            """
            {
              childEntities (orderBy: {path: "Parent.Property"})
              {
                property
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        var entity1 = new ChildEntity
        {
            Property = "Value2",
            Parent = parent
        };
        var entity2 = new ChildEntity
        {
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, entity1, entity2]);
    }

    [Fact]
    public async Task Like()
    {
        var query =
            """
            {
              parentEntities (where: {path: "Property", comparison: like, value: "value2"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_with_variable()
    {
        var query =
            """
            query ($value: String!)
            {
              parentEntities (where: {path: "Property", comparison: equal, value: [$value]})
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
                "value", "value2"
            }
        });
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_with_variable_notEqual()
    {
        var query =
            """
            query ($value: String!)
            {
              parentEntities (where: {path: "Property", comparison: notEqual, value: [$value]})
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
                "value", "value2"
            }
        });
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task CustomType()
    {
        var query =
            """
            {
              customType (orderBy: {path: "property"})
              {
                property
              }
            }
            """;

        var entity1 = new CustomTypeEntity
        {
            Property = long.MaxValue
        };
        var entity2 = new CustomTypeEntity
        {
            Property = 3
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Single_NotFound()
    {
        var query =
            """
            {
              parentEntity(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task First_NotFound()
    {
        var query =
            """
            {
              parentEntityFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task Single_Found_NoTracking()
    {
        var query =
            """
            {
              parentEntity(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, true, [entity1, entity2]);
    }

    [Fact]
    public async Task First_Found_NoTracking()
    {
        var query =
            """
            {
              parentEntityFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, true, [entity1, entity2]);
    }

    [Fact]
    public async Task Owned()
    {
        var query =
            """
            {
              ownedParent(id: "00000000-0000-0000-0000-000000000001") {
                property
                child1 {
                  property
                }
              }
            }
            """;
        var entity1 = new OwnedParent
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Parent value",
            Child1 = new()
            {
                Property = "Value1"
            },
            Child2 = new()
            {
                Property = "Value2"
            }
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1]);
    }

    [Fact]
    public async Task Explicit_Null()
    {
        var query =
            """
            {
              parentEntity(id: null) {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task Single_Found()
    {
        var query =
            """
            {
              parentEntity(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task First_Found()
    {
        var query =
            """
            {
              parentEntityFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Single_NoArgs()
    {
        var query =
            """
            {
              parentEntityWithNoArgs {
                property
                children
                {
                  property
                }
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task First_NoArgs()
    {
        var query =
            """
            {
              parentEntityWithNoArgsFirst {
                property
                children
                {
                  property
                }
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Single_IdOnly()
    {
        var query =
            """
            {
              parentEntityIdOnly(id: "00000000-0000-0000-0000-000000000001") {
                property
                children
                {
                  property
                }
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task First_IdOnly()
    {
        var query =
            """
            {
              parentEntityIdOnlyFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
                children
                {
                  property
                }
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task SingleNullable_NotFound()
    {
        var query =
            """
            {
              parentEntityNullable(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task FirstNullable_NotFound()
    {
        var query =
            """
            {
              parentEntityNullableFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task SingleNullable_Found()
    {
        var query =
            """
            {
              parentEntityNullable(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task FirstNullable_Found()
    {
        var query =
            """
            {
              parentEntityNullableFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task SingleParent_Child_mutation()
    {
        var query =
            """
            mutation {
              parentEntityMutation(id: "00000000-0000-0000-0000-000000000001") {
                property
                children(orderBy: {path: "property"})
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task FirstParent_Child_mutation()
    {
        var query =
            """
            mutation {
              parentEntityMutationFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
                children(orderBy: {path: "property"})
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task SingleParent_Child()
    {
        var query =
            """
            {
              parentEntity(id: "00000000-0000-0000-0000-000000000001") {
                property
                children(orderBy: {path: "property"})
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task FirstParent_Child()
    {
        var query =
            """
            {
              parentEntityFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
                children(orderBy: {path: "property"})
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task SingleParent_Child_WithFragment()
    {
        var query =
            """
            {
              parentEntity(id: "00000000-0000-0000-0000-000000000001") {
                ...parentEntityFields
              }
            }
            fragment parentEntityFields on Parent {
              property
              children(orderBy: {path: "property"})
              {
                ...childEntityFields
              }
            }
            fragment childEntityFields on Child {
              property
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task FirstParent_Child_WithFragment()
    {
        var query =
            """
            {
              parentEntityFirst(id: "00000000-0000-0000-0000-000000000001") {
                ...parentEntityFields
              }
            }
            fragment parentEntityFields on Parent {
              property
              children(orderBy: {path: "property"})
              {
                ...childEntityFields
              }
            }
            fragment childEntityFields on Child {
              property
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task Where()
    {
        var query =
            """
            {
              parentEntities (where: {path: "Property", comparison: equal, value: "value2"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_notEqual()
    {
        var query =
            """
            {
              parentEntities (where: {path: "Property", comparison: notEqual, value: "value2"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Where_default_comparison()
    {
        var query =
            """
            {
              parentEntities (where: {path: "Property", value: "value2"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Id()
    {
        var query =
            """
            {
              parentEntities (ids: "00000000-0000-0000-0000-000000000001")
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task NamedId()
    {
        var query =
            """
            {
              namedEntities (ids: "00000000-0000-0000-0000-000000000001")
              {
                property
              }
            }
            """;

        var entity1 = new NamedIdEntity
        {
            NamedId = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new NamedIdEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task Id_multiple()
    {
        var query =
            """
            {
              parentEntities
              (ids: ["00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002"])
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        var entity3 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Value3"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task In()
    {
        var query =
            """
            {
              parentEntities (where: {path: "Property", comparison: in, value: "value2"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact]
    public async Task In_multiple()
    {
        var query =
            """
            {
              parentEntities
              (where: {path: "Property", comparison: in, value: ["Value1", "Value2"]}, orderBy: {path: "property"})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    [Fact(Skip = "Work out why include is not used")]
    public async Task Connection_parent_child()
    {
        var query =
            """
            {
              parentEntitiesConnection(first:2, after: "0") {
                totalCount
                edges {
                  cursor
                  node {
                    property
                    children
                    {
                      property
                    }
                  }
                }
                items {
                  property
                  children
                  {
                    property
                  }
                }
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

    [Fact]
    public async Task Child_parent_with_alias()
    {
        var query =
            """
            {
              childEntities (orderBy: {path: "property"})
              {
                parentAlias
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact(Skip = "TODO")]
    public async Task Multiple_nested_AddQueryField()
    {
        var query =
            """
            {
              queryFieldWithInclude
              {
                includeNonQueryableB
                {
                  id
                }
              }
            }
            """;
        var level2 = new IncludeNonQueryableA();
        var level1 = new IncludeNonQueryableB
        {
            IncludeNonQueryableA = level2,
        };
        level1.IncludeNonQueryableA = level2;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [level1, level2]);
    }

    [Fact(Skip = "fix order")]
    public async Task Skip_level()
    {
        var query =
            """
            {
              skipLevel
              {
                level3Entity
                {
                  property
                }
              }
            }
            """;

        var level3 = new Level3Entity
        {
            Property = "Value"
        };
        var level2 = new Level2Entity
        {
            Level3Entity = level3
        };
        var level1 = new Level1Entity
        {
            Level2Entity = level2
        };

        var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [level1, level2, level3]);
    }

    [Fact]
    public async Task Multiple_nested()
    {
        var query =
            """
            {
              level1Entities
              {
                level2Entity
                {
                  level3Entity
                  {
                    property
                  }
                }
              }
            }
            """;

        var level3 = new Level3Entity
        {
            Property = "Value"
        };
        var level2 = new Level2Entity
        {
            Level3Entity = level3
        };
        var level1 = new Level1Entity
        {
            Level2Entity = level2
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [level1, level2, level3]);
    }

    [Fact]
    public async Task Null_on_nested()
    {
        var query =
            """
            {
              level1Entities(where: {path: "Level2Entity.Level3EntityId", comparison: equal, value: "00000000-0000-0000-0000-000000000003"})
              {
                level2Entity
                {
                  level3Entity
                  {
                    property
                  }
                }
              }
            }
            """;

        var level3a = new Level3Entity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Valuea"
        };
        var level2a = new Level2Entity
        {
            Level3Entity = level3a
        };
        var level1a = new Level1Entity
        {
            Level2Entity = level2a
        };

        var level2b = new Level2Entity();
        var level1b = new Level1Entity
        {
            Level2Entity = level2b
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [level1b, level2b, level1a, level2a, level3a]);
    }

    [Fact]
    public async Task Null_on_nested_notEqual()
    {
        var query =
            """
            {
              level1Entities(where: {path: "Level2Entity.Level3EntityId", comparison: notEqual, value: "00000000-0000-0000-0000-000000000003"})
              {
                level2Entity
                {
                  level3Entity
                  {
                    property
                  }
                }
              }
            }
            """;

        var level3a = new Level3Entity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Valuea"
        };
        var level2a = new Level2Entity
        {
            Level3Entity = level3a
        };
        var level1a = new Level1Entity
        {
            Level2Entity = level2a
        };

        var level2b = new Level2Entity();
        var level1b = new Level1Entity
        {
            Level2Entity = level2b
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [level1b, level2b, level1a, level2a, level3a]);
    }

    [Fact]
    public async Task Query_Cyclic()
    {
        var query =
            """
            {
              childEntities (orderBy: {path: "property"})
              {
                property
                parent
                {
                  property
                  children
                  {
                    property
                    parent
                    {
                      property
                    }
                  }
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task Query_NoTracking()
    {
        var query =
            """
            {
              childEntities (orderBy: {path: "property"})
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, true, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task Child_parent()
    {
        var query =
            """
            {
              childEntities (orderBy: {path: "property"})
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task With_null_navigation_property()
    {
        var query =
            """
            {
              childEntities(where: {path: "ParentId", comparison: equal, value: "00000000-0000-0000-0000-000000000001"}, orderBy: {path: "property"})
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity5 = new ChildEntity
        {
            Property = "Value5"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity5]);
    }

    [Fact]
    public async Task With_null_navigation_property_notEqual()
    {
        var query =
            """
            {
              childEntities(where: {path: "ParentId", comparison: notEqual, value: "00000000-0000-0000-0000-000000000001"}, orderBy: {path: "property"})
              {
                property
                parent
                {
                  property
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity5 = new ChildEntity
        {
            Property = "Value5"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity5]);
    }

    [Fact(Skip = "fix order")]
    public async Task MisNamedQuery()
    {
        var query =
            """
            {
              misNamed
              {
                misNamedChildren
                {
                  id
                }
              }
            }
            """;

        var entity1 = new WithMisNamedQueryParentEntity();
        var entity2 = new WithMisNamedQueryChildEntity
        {
            Parent = entity1
        };
        var entity3 = new WithMisNamedQueryChildEntity
        {
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new WithMisNamedQueryParentEntity();
        var entity5 = new WithMisNamedQueryChildEntity
        {
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact(Skip = "fix order")]
    public async Task Parent_child()
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
                }
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3, entity4, entity5]);
    }

    [Fact]
    public async Task Parent_child_with_id()
    {
        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent
        };
        parent.Children.Add(child1);
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = parent
        };
        parent.Children.Add(child2);

        var query = $$"""
                      {
                        parentEntities
                        {
                          property
                          children(id:"{{child1.Id}}" )
                          {
                            property
                          }
                        }
                      }
                      """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact(Skip = "fix order")]
    public async Task Parent_with_id_child()
    {
        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Value2"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        parent1.Children.Add(child1);
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = parent1
        };
        parent1.Children.Add(child2);

        var query = $$"""
                      {
                        parentEntities(id:'{{parent1.Id}}')
                        {
                          property
                          children
                          {
                            property
                          }
                        }
                      }
                      """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent1, parent2, child1, child2]);
    }

    [Fact]
    public async Task Parent_with_id_child_with_id()
    {
        var parent1 = new ParentEntity
        {
            Property = "Value1"
        };
        var parent2 = new ParentEntity
        {
            Property = "Value2"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        parent1.Children.Add(child1);
        var child2 = new ChildEntity
        {
            Property = "Child2",
            Parent = parent1
        };
        parent1.Children.Add(child2);

        var query = $$"""
                      {
                        parentEntities(id:"{{parent1.Id}}")
                        {
                          property
                          children(id:"{{child1.Id}}" )
                          {
                            property
                          }
                        }
                      }
                      """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent1, parent2, child1, child2]);
    }

    [Fact]
    public async Task Many_children()
    {
        var query =
            """
            {
              manyChildren
              {
                child1
                {
                  id
                }
              }
            }
            """;

        var parent = new WithManyChildrenEntity();
        var child1 = new Child1Entity
        {
            Parent = parent
        };
        var child2 = new Child2Entity
        {
            Parent = parent
        };
        parent.Child1 = child1;
        parent.Child2 = child2;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact(Skip = "Broke with gql 4")]
    public async Task InheritedEntityInterface()
    {
        var query =
            """
            {
              interfaceGraphConnection {
                items {
                  ...inheritedEntityFields
                }
              }
            }
            fragment inheritedEntityFields on Interface {
              property
              childrenFromInterface(orderBy: {path: "property"})
              {
                items {
                  ...childEntityFields
                }
              }
              ... on DerivedWithNavigation {
                childrenFromDerived(orderBy: {path: "property"})
                {
                  items {
                    ...childEntityFields
                  }
                }
              }
            }
            fragment childEntityFields on DerivedChild {
              property
            }
            """;

        var derivedEntity1 = new DerivedEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var childEntity1 = new DerivedChildEntity
        {
            Property = "Value2",
            Parent = derivedEntity1
        };
        var childEntity2 = new DerivedChildEntity
        {
            Property = "Value3",
            Parent = derivedEntity1
        };
        derivedEntity1.ChildrenFromBase.Add(childEntity1);
        derivedEntity1.ChildrenFromBase.Add(childEntity2);

        var derivedEntity2 = new DerivedWithNavigationEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value4"
        };
        var childEntity3 = new DerivedChildEntity
        {
            Property = "Value5",
            Parent = derivedEntity2
        };
        var childEntity4 = new DerivedChildEntity
        {
            Property = "Value6",
            TypedParent = derivedEntity2
        };
        derivedEntity2.ChildrenFromBase.Add(childEntity3);
        derivedEntity2.Children.Add(childEntity4);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [derivedEntity1, childEntity1, childEntity2, derivedEntity2, childEntity3, childEntity4]);
    }

    [Fact]
    public async Task ManyToManyRightWhereAndInclude()
    {
        var query =
            """
            {
              manyToManyLeftEntities (where: {path: "rights[rightName]", comparison: equal, value: "Right2"})
              {
                leftName
                rights
                {
                  rightName
                }
              }
            }
            """;

        var middle11 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left1Id",
                LeftName = "Left1"
            },
            ManyToManyRightEntity = new()
            {
                Id = "Right1Id",
                RightName = "Right1"
            }
        };

        var middle12 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = middle11.ManyToManyLeftEntity,
            ManyToManyRightEntity = new()
            {
                Id = "Right2Id",
                RightName = "Right2"
            }
        };

        var middle21 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left2Id",
                LeftName = "Left2"
            },
            ManyToManyRightEntity = middle11.ManyToManyRightEntity
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [middle11, middle12, middle21]);
    }

    [Fact]
    public async Task ManyToManyRightWhereAndInclude_notEqual()
    {
        var query =
            """
            {
              manyToManyLeftEntities (where: {path: "rights[rightName]", comparison: notEqual, value: "Right2"})
              {
                leftName
                rights
                {
                  rightName
                }
              }
            }
            """;

        var middle11 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left1Id",
                LeftName = "Left1"
            },
            ManyToManyRightEntity = new()
            {
                Id = "Right1Id",
                RightName = "Right1"
            }
        };

        var middle12 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = middle11.ManyToManyLeftEntity,
            ManyToManyRightEntity = new()
            {
                Id = "Right2Id",
                RightName = "Right2"
            }
        };

        var middle21 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left2Id",
                LeftName = "Left2"
            },
            ManyToManyRightEntity = middle11.ManyToManyRightEntity
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [middle11, middle12, middle21]);
    }

    [Fact]
    public async Task ManyToManyLeftWhereAndInclude()
    {
        var query =
            """
            {
              manyToManyRightEntities (where: {path: "lefts[leftName]", comparison: equal, value: "Left2"})
              {
                rightName
                lefts
                {
                  leftName
                }
              }
            }
            """;

        var middle11 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left1Id",
                LeftName = "Left1"
            },
            ManyToManyRightEntity = new()
            {
                Id = "Right1Id",
                RightName = "Right1"
            }
        };

        var middle12 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = middle11.ManyToManyLeftEntity,
            ManyToManyRightEntity = new()
            {
                Id = "Right2Id",
                RightName = "Right2"
            }
        };

        var middle21 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left2Id",
                LeftName = "Left2"
            },
            ManyToManyRightEntity = middle11.ManyToManyRightEntity
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [middle11, middle12, middle21]);
    }

    [Fact]
    public async Task ManyToManyLeftWhereAndInclude_notEqual()
    {
        var query =
            """
            {
              manyToManyRightEntities (where: {path: "lefts[leftName]", comparison: notEqual, value: "Left2"})
              {
                rightName
                lefts
                {
                  leftName
                }
              }
            }
            """;

        var middle11 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left1Id",
                LeftName = "Left1"
            },
            ManyToManyRightEntity = new()
            {
                Id = "Right1Id",
                RightName = "Right1"
            }
        };

        var middle12 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = middle11.ManyToManyLeftEntity,
            ManyToManyRightEntity = new()
            {
                Id = "Right2Id",
                RightName = "Right2"
            }
        };

        var middle21 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left2Id",
                LeftName = "Left2"
            },
            ManyToManyRightEntity = middle11.ManyToManyRightEntity
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [middle11, middle12, middle21]);
    }

    static async Task RunQuery(
        SqlDatabase<IntegrationDbContext> database,
        string query,
        Inputs? inputs,
        Filters<IntegrationDbContext>? filters,
        bool disableTracking,
        object[] entities,
        [CallerFilePath] string sourceFile = "")
    {
        var dbContext = database.Context;
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton<Mutation>();
        services.AddSingleton(database.Context);
        services.AddGraphQL(null);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        await using var context = database.NewDbContext();
        Recording.Start();
        string result;
        try
        {
            result = await QueryExecutor.ExecuteQuery(query, services, context, inputs, filters, disableTracking);
        }
        catch (ExecutionError executionError)
        {
            await Verify(executionError.Message, sourceFile: sourceFile)
                .IgnoreStackTrace();
            return;
        }
        catch (Exception exception)
        {
            await Verify(exception, sourceFile: sourceFile)
                .IgnoreStackTrace();
            return;
        }

        await Verify(result, sourceFile: sourceFile)
            .ScrubInlineGuids();
    }

    static IEnumerable<Type> GetGraphQlTypes() =>
        typeof(IntegrationTests)
            .Assembly
            .GetTypes()
            .Where(_ => !_.IsAbstract && _.IsAssignableTo<GraphType>());

    [Fact]
    public async Task IQueryableFirst()
    {
        var query =
            """
            {
              iQueryableFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task IQueryableSingle()
    {
        var query =
            """
            {
              iQueryableSingle(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task IQueryable()
    {
        var query =
            """
            {
              iQueryable(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task SingleNullQueryableSingle()
    {
        var query =
            """
            {
              nullQueryableSingle(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullQueryableFirst()
    {
        var query =
            """
            {
              nullQueryableFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskQueryableSingle()
    {
        var query =
            """
            {
              nullTaskQueryableSingle(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskInnerQueryableSingle()
    {
        var query =
            """
            {
              nullTaskInnerQueryableSingle(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskQueryableFirst()
    {
        var query =
            """
            {
              nullTaskQueryableFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskInnerQueryableFirst()
    {
        var query =
            """
            {
              nullTaskInnerQueryableFirst(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task SingleNullQueryableSingleDisallowNull()
    {
        var query =
            """
            {
              nullQueryableSingleDisallowNull(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullQueryableFirstDisallowNull()
    {
        var query =
            """
            {
              nullQueryableFirstDisallowNull(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskQueryableSingleDisallowNull()
    {
        var query =
            """
            {
              nullTaskQueryableSingleDisallowNull(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskInnerQueryableSingleDisallowNull()
    {
        var query =
            """
            {
              nullTaskInnerQueryableSingleDisallowNull(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskQueryableFirstDisallowNull()
    {
        var query =
            """
            {
              nullTaskQueryableFirstDisallowNull(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullTaskInnerQueryableFirstDisallowNull()
    {
        var query =
            """
            {
              nullTaskInnerQueryableFirstDisallowNull(id: "00000000-0000-0000-0000-000000000001") {
                property
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    [Fact]
    public async Task NullQueryConnection()
    {
        var query =
            """
            {
              nullQueryConnection(first:2, after: "0") {
                totalCount
                edges {
                  cursor
                  node {
                    property
                  }
                }
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
    public async Task NullTaskQueryConnection()
    {
        var query =
            """
            {
              nullTaskQueryConnection(first:2, after: "0") {
                totalCount
                edges {
                  cursor
                  node {
                    property
                  }
                }
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
    public async Task NullTaskInnerQueryConnection()
    {
        var query =
            """
            {
              nullTaskInnerQueryConnection(first:2, after: "0") {
                totalCount
                edges {
                  cursor
                  node {
                    property
                  }
                }
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
}