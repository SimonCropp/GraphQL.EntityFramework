public partial class IntegrationTests
{
    #region ProjectedSingleField tests

    [Fact]
    public async Task Projected_single_field_basic()
    {
        var query = """
            {
              projectedParentSingle(id: "00000000-0000-0000-0000-000000000001")
            }
            """;

        var entity = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "test"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_single_field_async()
    {
        var query = """
            {
              projectedParentSingleAsync(id: "00000000-0000-0000-0000-000000000001")
            }
            """;

        var entity = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "test"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_single_field_nullable_returns_null()
    {
        var query = """
            {
              projectedParentSingleNullable(id: "00000000-0000-0000-0000-000000000099")
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    #endregion

    #region ProjectedFirstField tests

    [Fact]
    public async Task Projected_first_field_basic()
    {
        var query = """
            {
              projectedParentFirst(id: "00000000-0000-0000-0000-000000000001")
            }
            """;

        var entity = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "test"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_first_field_async()
    {
        var query = """
            {
              projectedParentFirstAsync(id: "00000000-0000-0000-0000-000000000001")
            }
            """;

        var entity = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "test"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_first_field_nullable_returns_null()
    {
        var query = """
            {
              projectedParentFirstNullable(id: "00000000-0000-0000-0000-000000000099")
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, []);
    }

    #endregion

    #region ProjectedQueryField tests

    [Fact]
    public async Task Projected_query_field_list()
    {
        var query = """
            {
              projectedParentQuery
            }
            """;

        var entities = new[]
        {
            new ParentEntity { Property = "first" },
            new ParentEntity { Property = "second" }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }

    [Fact]
    public async Task Projected_query_field_list_async()
    {
        var query = """
            {
              projectedParentQueryAsync
            }
            """;

        var entities = new[]
        {
            new ParentEntity { Property = "first" },
            new ParentEntity { Property = "second" }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }

    #endregion

    #region ProjectedQueryConnectionField tests

    [Fact]
    public async Task Projected_query_connection_field()
    {
        var query = """
            {
              projectedParentQueryConnection(first: 2) {
                totalCount
                edges {
                  node
                  cursor
                }
                pageInfo {
                  hasNextPage
                  hasPreviousPage
                }
              }
            }
            """;

        var entities = new[]
        {
            new ParentEntity { Property = "alpha" },
            new ParentEntity { Property = "beta" },
            new ParentEntity { Property = "gamma" }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }

    [Fact]
    public async Task Projected_query_connection_field_async()
    {
        var query = """
            {
              projectedParentQueryConnectionAsync(first: 2) {
                totalCount
                edges {
                  node
                  cursor
                }
                pageInfo {
                  hasNextPage
                  hasPreviousPage
                }
              }
            }
            """;

        var entities = new[]
        {
            new ParentEntity { Property = "alpha" },
            new ParentEntity { Property = "beta" },
            new ParentEntity { Property = "gamma" }
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities);
    }

    #endregion

    #region ProjectedNavigationConnectionField tests

    [Fact]
    public async Task Projected_navigation_connection_field()
    {
        var query = """
            {
              parentEntities {
                id
                childrenConnectionProjected(first: 2) {
                  totalCount
                  edges {
                    node
                    cursor
                  }
                  pageInfo {
                    hasNextPage
                    hasPreviousPage
                  }
                }
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "parent",
            Children =
            [
                new() { Property = "child1" },
                new() { Property = "child2" },
                new() { Property = "child3" }
            ]
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    [Fact]
    public async Task Projected_navigation_connection_field_async()
    {
        var query = """
            {
              parentEntities {
                id
                childrenConnectionProjectedAsync(first: 2) {
                  totalCount
                  edges {
                    node
                    cursor
                  }
                  pageInfo {
                    hasNextPage
                    hasPreviousPage
                  }
                }
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "parent",
            Children =
            [
                new() { Property = "child1" },
                new() { Property = "child2" },
                new() { Property = "child3" }
            ]
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity]);
    }

    #endregion
}
