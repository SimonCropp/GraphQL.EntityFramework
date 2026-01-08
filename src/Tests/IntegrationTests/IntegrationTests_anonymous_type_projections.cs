public partial class IntegrationTests
{
    [Fact]
    public async Task Filter_using_anonymous_type_projection_with_For()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                intValue
                boolValue
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "FailsIntCheck",
            IntValue = 5,
            BoolValue = true
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "FailsBoolCheck",
            IntValue = 15,
            BoolValue = false
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "PassesBoth",
            IntValue = 20,
            BoolValue = true
        };
        var entity4 = new SimpleTypeFilterEntity
        {
            Name = "AlsoPassesBoth",
            IntValue = 30,
            BoolValue = true
        };

        var filters = new Filters<IntegrationDbContext>();

        // Using For<T>() to enable anonymous type projection
        // TProjection is inferred from the projection expression
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: e => new { e.IntValue, e.BoolValue },
            filter: (_, _, _, projected) =>
                projected.IntValue >= 10 && projected.BoolValue);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3, entity4]);
    }

    [Fact]
    public async Task Filter_using_anonymous_type_with_async_filter()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                intValue
                guidValue
              }
            }
            """;

        var allowedGuid = Guid.NewGuid();

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "FailsIntCheck",
            IntValue = 5,
            GuidValue = allowedGuid
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "FailsGuidCheck",
            IntValue = 15,
            GuidValue = Guid.NewGuid()
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "PassesBoth",
            IntValue = 25,
            GuidValue = allowedGuid
        };

        var filters = new Filters<IntegrationDbContext>();

        // Using For<T>() with async filter and anonymous type
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: e => new { e.IntValue, e.GuidValue },
            filter: async (_, _, _, projected) =>
            {
                await Task.Delay(1); // Simulate async work
                return projected.IntValue >= 10 && projected.GuidValue == allowedGuid;
            });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_anonymous_type_with_three_properties()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                intValue
                boolValue
              }
            }
            """;

        var cutoffDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "TooOld",
            IntValue = 20,
            BoolValue = true,
            DateTimeValue = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "Inactive",
            IntValue = 20,
            BoolValue = false,
            DateTimeValue = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "TooSmall",
            IntValue = 5,
            BoolValue = true,
            DateTimeValue = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var entity4 = new SimpleTypeFilterEntity
        {
            Name = "Perfect",
            IntValue = 25,
            BoolValue = true,
            DateTimeValue = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var filters = new Filters<IntegrationDbContext>();

        // Anonymous type with three properties
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: e => new
            {
                e.IntValue,
                e.BoolValue,
                e.DateTimeValue
            },
            filter: (_, _, _, x) =>
                x.IntValue >= 10 &&
                x.BoolValue &&
                x.DateTimeValue >= cutoffDate);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3, entity4]);
    }

    [Fact]
    public async Task Filter_using_anonymous_type_with_nested_navigation_property()
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
                  isActive
                }
              }
            }
            """;

        var parent1 = new FilterParentEntity
        {
            Property = "AllowedParent"
        };
        var parent2 = new FilterParentEntity
        {
            Property = "BlockedParent"
        };

        var child1 = new FilterChildEntity
        {
            Property = "Child1",
            Age = 25,
            IsActive = true,
            Parent = parent1
        };
        var child2 = new FilterChildEntity
        {
            Property = "Child2",
            Age = 30,
            IsActive = false,
            Parent = parent1
        };
        var child3 = new FilterChildEntity
        {
            Property = "Child3",
            Age = 35,
            IsActive = true,
            Parent = parent2
        };
        var child4 = new FilterChildEntity
        {
            Property = "Child4",
            Age = 40,
            IsActive = true,
            Parent = parent1
        };

        parent1.Children.Add(child1);
        parent1.Children.Add(child2);
        parent1.Children.Add(child4);
        parent2.Children.Add(child3);

        var filters = new Filters<IntegrationDbContext>();

        // Anonymous type with nested navigation property access
        // Filter child entities based on their own properties + parent's property
        filters.For<FilterChildEntity>().Add(
            projection: c => new
            {
                c.Age,
                c.IsActive,
                ParentProperty = c.Parent!.Property
            },
            filter: (_, _, _, x) =>
                x.Age >= 30 &&
                x.IsActive &&
                x.ParentProperty == "AllowedParent");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent1, parent2, child1, child2, child3, child4]);
    }
}
