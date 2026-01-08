public partial class IntegrationTests
{
    [Fact]
    public async Task Child_filtered()
    {
        var query =
            """
            {
              parentEntitiesFiltered
              {
                property
                children
                {
                  property
                }
              }
            }
            """;

        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "Ignore",
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [entity1, entity2, entity3]);
    }

    static Filters<IntegrationDbContext> BuildFilters()
    {
        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterParentEntity>().Add(
            e => new FilterProjection
                { Id = e.Id, Property = e.Property },
            (_, _, _, item) => item.Property != "Ignore");
        filters.For<FilterChildEntity>().Add(
            e => new FilterProjection
                { Id = e.Id, Property = e.Property },
            (_, _, _, item) => item.Property != "Ignore");
        filters.For<Level3Entity>().Add(
            e => new FilterProjection
                { Id = e.Id, Property = e.Property },
            (_, _, _, item) => item.Property != "Ignore");
        return filters;
    }

    class FilterProjection
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
    }

    [Fact]
    public async Task RootList_filtered()
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

        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterParentEntity
        {
            Property = "Ignore"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [entity1, entity2]);
    }

    [Fact]
    public async Task Root_connectionFiltered()
    {
        var query =
            """
            {
              parentEntitiesConnectionFiltered {
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
        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterParentEntity
        {
            Property = "Ignore"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [entity1, entity2]);
    }

    [Fact]
    public async Task Connection_parent_child_Filtered()
    {
        var query =
            """
            {
              parentEntitiesConnectionFiltered {
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
        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "Ignore"
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Value3"
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Multiple_nested_Filtered()
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
            Property = "Ignore"
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
        await RunQuery(database, query, null, BuildFilters(), false, [level1, level2, level3]);
    }
}
