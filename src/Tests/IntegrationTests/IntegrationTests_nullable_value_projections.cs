public partial class IntegrationTests
{
    [Fact]
    public async Task Filter_nullable_int_projection_with_value()
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
                  nullableAge
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
            Property = "HasNullAge",
            NullableAge = null,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "HasAge",
            NullableAge = 30,
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.NullableAge,
            filter: (_, _, _, age) => age is >= 18);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_nullable_int_projection_is_null()
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
                  nullableAge
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
            Property = "HasNullAge",
            NullableAge = null,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "HasAge",
            NullableAge = 30,
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.NullableAge,
            filter: (_, _, _, age) => !age.HasValue);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_nullable_bool_projection()
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
                  nullableIsActive
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
            Property = "NullActive",
            NullableIsActive = null,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Active",
            NullableIsActive = true,
            Parent = entity1
        };
        var entity4 = new FilterChildEntity
        {
            Property = "Inactive",
            NullableIsActive = false,
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        entity1.Children.Add(entity4);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.NullableIsActive,
            filter: (_, _, _, isActive) => isActive == true);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3, entity4]);
    }

    [Fact]
    public async Task Filter_nullable_datetime_projection()
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
                  nullableCreatedAt
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
            Property = "NullDate",
            NullableCreatedAt = null,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "TooOld",
            NullableCreatedAt = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Parent = entity1
        };
        var entity4 = new FilterChildEntity
        {
            Property = "Recent",
            NullableCreatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        entity1.Children.Add(entity4);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.NullableCreatedAt,
            filter: (_, _, _, createdAt) => createdAt.HasValue && createdAt.Value >= cutoffDate);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3, entity4]);
    }

    [Fact]
    public async Task Filter_nullable_string_is_not_null()
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
            Property = null,
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "HasValue",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var filters = new Filters<IntegrationDbContext>();
        filters.For<FilterChildEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != null);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }
}
