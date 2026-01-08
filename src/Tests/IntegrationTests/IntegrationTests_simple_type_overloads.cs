public partial class IntegrationTests
{
    [Fact]
    public async Task Filter_using_int_overload()
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
            Name = "TooSmall",
            IntValue = 5
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "JustRight",
            IntValue = 15
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "TooBig",
            IntValue = 100
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using For<T>() - TProjection is inferred
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.IntValue,
            filter: (_, _, _, value) => value is >= 10 and <= 50);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_nullable_int_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                nullableIntValue
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "HasNull",
            NullableIntValue = null
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "HasValue",
            NullableIntValue = 42
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "TooSmall",
            NullableIntValue = 5
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using For<T>() for nullable int
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.NullableIntValue,
            filter: (_, _, _, value) => value is >= 20);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_datetime_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                dateTimeValue
              }
            }
            """;

        var cutoffDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "TooOld",
            DateTimeValue = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "Recent",
            DateTimeValue = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "WayTooOld",
            DateTimeValue = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for DateTime
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.DateTimeValue,
            filter: (_, _, _, value) => value >= cutoffDate);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_nullable_datetime_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                nullableDateTimeValue
              }
            }
            """;

        var cutoffDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "NoDate",
            NullableDateTimeValue = null
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "Recent",
            NullableDateTimeValue = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "TooOld",
            NullableDateTimeValue = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for nullable DateTime
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.NullableDateTimeValue,
            filter: (_, _, _, value) => value.HasValue && value.Value >= cutoffDate);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_bool_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                boolValue
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "IsTrue",
            BoolValue = true
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "IsFalse",
            BoolValue = false
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "AlsoFalse",
            BoolValue = false
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for bool
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.BoolValue,
            filter: (_, _, _, value) => value);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_nullable_bool_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                nullableBoolValue
              }
            }
            """;

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "IsNull",
            NullableBoolValue = null
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "IsTrue",
            NullableBoolValue = true
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "IsFalse",
            NullableBoolValue = false
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for nullable bool
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.NullableBoolValue,
            filter: (_, _, _, value) => value == true);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_guid_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                guidValue
              }
            }
            """;

        var allowedGuid = Guid.NewGuid();

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "Allowed",
            GuidValue = allowedGuid
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "NotAllowed1",
            GuidValue = Guid.NewGuid()
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "NotAllowed2",
            GuidValue = Guid.NewGuid()
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for Guid
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.GuidValue,
            filter: (_, _, _, value) => value == allowedGuid);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_nullable_guid_overload()
    {
        var query =
            """
            {
              simpleTypeFilterEntities
              {
                name
                nullableGuidValue
              }
            }
            """;

        var allowedGuid = Guid.NewGuid();

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "HasNull",
            NullableGuidValue = null
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "HasAllowedValue",
            NullableGuidValue = allowedGuid
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "HasOtherValue",
            NullableGuidValue = Guid.NewGuid()
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for nullable Guid
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.NullableGuidValue,
            filter: (_, _, _, value) => value.HasValue && value.Value == allowedGuid);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_string_overload()
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

        var entity1 = new SimpleTypeFilterEntity
        {
            Name = "Alpha"
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "Beta"
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "Gamma"
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload for string?
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.Name,
            filter: (_, _, _, value) => value != "Beta");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Filter_using_async_int_overload()
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
            Name = "TooSmall",
            IntValue = 5
        };
        var entity2 = new SimpleTypeFilterEntity
        {
            Name = "JustRight",
            IntValue = 15
        };
        var entity3 = new SimpleTypeFilterEntity
        {
            Name = "TooBig",
            IntValue = 100
        };

        var filters = new Filters<IntegrationDbContext>();
        // Using simple type overload with AsyncFilter - demonstrates async filter usage
        filters.For<SimpleTypeFilterEntity>().Add(
            projection: _ => _.IntValue,
            filter: async (_, _, _, value) =>
            {
                await Task.Delay(1); // Simulate async work
                return value is >= 10 and <= 50;
            });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [entity1, entity2, entity3]);
    }
}
