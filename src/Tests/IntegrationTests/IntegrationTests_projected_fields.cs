public partial class IntegrationTests
{
    [Fact]
    public async Task Projected_navigation_field_simple_transform()
    {
        var query = """
            {
              parentEntities {
                id
                propertyUpper
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "value"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_navigation_field_async_transform()
    {
        var query = """
            {
              parentEntities {
                id
                propertyUpperAsync
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "value"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_navigation_field_context_aware_transform()
    {
        var query = """
            {
              parentEntities {
                id
                propertyWithContext
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "value"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_navigation_field_null_handling()
    {
        var query = """
            {
              parentEntities {
                id
                propertyUpper
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = null
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_navigation_list_field()
    {
        var query = """
            {
              parentEntities {
                id
                childrenProperties
              }
            }
            """;

        var entity = new ParentEntity
        {
            Children =
            [
                new()
                {
                    Property = "child1"
                },
                new()
                {
                    Property = "child2"
                }
            ]
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_query_field()
    {
        var query = """
            {
              projectedParents
            }
            """;

        var entity = new ParentEntity
        {
            Property = "TEST"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_field_with_filter()
    {
        var query = """
            {
              parentEntities(where: {path: "Property", comparison: equal, value: "value"}) {
                id
                propertyUpper
              }
            }
            """;

        var entities = new[]
        {
            new ParentEntity { Property = "value" },
            new ParentEntity { Property = "other" }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }

    [Fact]
    public async Task Projected_navigation_field_nested()
    {
        var query = """
            {
              level1Entities {
                id
                level2Property
              }
            }
            """;

        var entity = new Level1Entity
        {
            Level2Entity = new()
            {
                Level3Entity = new()
                {
                    Property = "nested"
                }
            }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_field_anonymous_type()
    {
        var query = """
            {
              parentEntities {
                id
                anonymousProjection
              }
            }
            """;

        var entity = new ParentEntity
        {
            Id = Guid.NewGuid(),
            Property = "test"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }
}
