public partial class IntegrationTests
{
    [Fact]
    public async Task Simplified_filter_sync_with_pk_access()
    {
        var query =
            """
            {
              childEntities
              {
                property
              }
            }
            """;

        var entity1 = new ChildEntity { Property = "Entity1" };
        var entity2 = new ChildEntity { Property = "Entity2" };
        var entity3 = new ChildEntity { Property = "Entity3" };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API - filter by primary key
        filters.For<ChildEntity>().Add(
            filter: (_, _, _, e) => e.Id == entity1.Id || e.Id == entity3.Id);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Simplified_filter_async_with_fk_access()
    {
        var parent1 = new ParentEntity { Property = "Parent1" };
        var parent2 = new ParentEntity { Property = "Parent2" };

        var query =
            """
            {
              childEntities
              {
                property
              }
            }
            """;

        var entity1 = new ChildEntity { Property = "Child1", Parent = parent1 };
        var entity2 = new ChildEntity { Property = "Child2", Parent = parent2 };
        var entity3 = new ChildEntity { Property = "Child3", Parent = parent1 };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API - async filter by foreign key
        filters.For<ChildEntity>().Add(
            filter: async (_, _, _, e) =>
            {
                await Task.Delay(1); // Simulate async validation
                return e.ParentId == parent1.Id;
            });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent1, parent2, entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Simplified_filter_multiple_fk_access()
    {
        var parent1 = new ParentEntity { Property = "Parent1" };
        var parent2 = new ParentEntity { Property = "Parent2" };
        var parent3 = new ParentEntity { Property = "Parent3" };

        var query =
            """
            {
              childEntities
              {
                property
              }
            }
            """;

        var entity1 = new ChildEntity { Property = "Child1", Parent = parent1 };
        var entity2 = new ChildEntity { Property = "Child2", Parent = parent2 };
        var entity3 = new ChildEntity { Property = "Child3", Parent = parent3 };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API - filter with OR logic on foreign keys
        filters.For<ChildEntity>().Add(
            filter: (_, _, _, e) => e.ParentId == parent1.Id || e.ParentId == parent3.Id);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent1, parent2, parent3, entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Simplified_filter_nullable_fk_handling()
    {
        var parent1 = new ParentEntity { Property = "Parent1" };

        var query =
            """
            {
              childEntities
              {
                property
              }
            }
            """;

        var entity1 = new ChildEntity { Property = "Child1", Parent = parent1 };
        var entity2 = new ChildEntity { Property = "Child2", ParentId = null };
        var entity3 = new ChildEntity { Property = "Child3", Parent = parent1 };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API - filter with nullable FK check
        filters.For<ChildEntity>().Add(
            filter: (_, _, _, e) => e.ParentId != null);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent1, entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Simplified_filter_combined_with_existing_filters()
    {
        var query =
            """
            {
              parentEntitiesFiltered
              {
                property
              }
            }
            """;

        var entity1 = new FilterParentEntity { Property = "Entity1" };
        var entity2 = new FilterParentEntity { Property = "Entity2" };
        var entity3 = new FilterParentEntity { Property = "Entity3" };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API filter
        filters.For<FilterParentEntity>().Add(
            filter: (_, _, _, e) => e.Id != entity2.Id);

        // Traditional API filter with projection
        filters.For<FilterParentEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, prop) => prop != "Entity3");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Simplified_filter_list_query_filtering()
    {
        var query =
            """
            {
              parentEntitiesFiltered
              {
                property
              }
            }
            """;

        var entity1 = new FilterParentEntity { Property = "Entity1" };
        var entity2 = new FilterParentEntity { Property = "Entity2" };
        var entity3 = new FilterParentEntity { Property = "Entity3" };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API on list query
        filters.For<FilterParentEntity>().Add(
            filter: (_, _, _, e) => e.Id == entity1.Id || e.Id == entity2.Id);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Simplified_filter_connection_query_filtering()
    {
        var query =
            """
            {
              parentEntitiesConnectionFiltered
              {
                edges
                {
                  node
                  {
                    property
                  }
                }
              }
            }
            """;

        var entity1 = new FilterParentEntity { Property = "Entity1" };
        var entity2 = new FilterParentEntity { Property = "Entity2" };
        var entity3 = new FilterParentEntity { Property = "Entity3" };

        var filters = new Filters<IntegrationDbContext>();
        // Simplified API on connection query
        filters.For<FilterParentEntity>().Add(
            filter: (_, _, _, e) => e.Id != entity2.Id);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }
}
