public partial class IntegrationTests
{
    [Fact]
    public async Task Filter_string_projection()
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
            Property = "IgnoreMe",
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "KeepMe",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.Property!,
            filter: (_, _, _, property) => property != "IgnoreMe");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_int_projection()
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
                  age
                }
              }
            }
            """;

        var entity1 = new FilterParentEntity
        {
            Property = "Parent"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "TooYoung",
            Age = 15,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "OldEnough",
            Age = 25,
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.Age,
            filter: (_, _, _, age) => age >= 18);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_bool_projection()
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
                  isActive
                }
              }
            }
            """;

        var entity1 = new FilterParentEntity
        {
            Property = "Parent"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "Inactive",
            IsActive = false,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Active",
            IsActive = true,
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.IsActive,
            filter: (_, _, _, isActive) => isActive);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_datetime_projection()
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
                  createdAt
                }
              }
            }
            """;

        var cutoffDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var entity1 = new FilterParentEntity
        {
            Property = "Parent"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "TooOld",
            CreatedAt = new(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Recent",
            CreatedAt = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.CreatedAt,
            filter: (_, _, _, createdAt) => createdAt >= cutoffDate);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_guid_projection()
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
            Property = "Parent"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "Child1",
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Child2",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var allowedId = entity3.Id;

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id == allowedId);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }
}
