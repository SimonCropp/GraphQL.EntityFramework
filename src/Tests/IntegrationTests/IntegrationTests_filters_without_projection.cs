public partial class IntegrationTests
{
    [Fact]
    public async Task Filter_without_projection_sync()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                intValue
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "Entity1",
            IntValue = 10
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "Entity2",
            IntValue = 20
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "Entity3",
            IntValue = 30
        };

        var filters = new Filters<IntegrationDbContext>();
        // Filter without projection - doesn't need entity data
        // This filter doesn't look at entity data, only context
        filters.For<SimpleTypeFilterEntity>().Add(
            filter: (_, _, _) => true); // Simple pass-through filter for testing

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_without_projection_async()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                intValue
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "Entity1",
            IntValue = 10
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "Entity2",
            IntValue = 20
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "Entity3",
            IntValue = 30
        };

        var filters = new Filters<IntegrationDbContext>();
        // Async filter without projection - doesn't need entity data
        filters.For<SimpleTypeFilterEntity>().Add(
            filter: async (_, _, _) =>
            {
                await Task.Delay(1); // Simulate async operation
                return true;
            });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_without_projection_denies_all()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity { Name = "Entity1" };
        var entity2 = new SimpleTypeFilterEntity { Name = "Entity2" };

        var filters = new Filters<IntegrationDbContext>();
        // Filter without projection that denies all
        filters.For<SimpleTypeFilterEntity>().Add(
            filter: (_, _, _) => false);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2]);
    }
}
