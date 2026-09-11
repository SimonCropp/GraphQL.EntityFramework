public partial class IntegrationTests
{
    // The `in` comparison was limited to a fixed set of types for the list Contains lookup, which
    // did not include Date and Time even though their values could be converted.
    [Fact]
    public async Task Where_in_on_date()
    {
        var query =
            """
            {
              dateEntities (where: {property: {in: ["2020-01-01", "2020-01-03"]}}, orderBy: property)
              {
                property
              }
            }
            """;

        var entity1 = new DateEntity
        {
            Property = new(2020, 1, 1)
        };
        var entity2 = new DateEntity
        {
            Property = new(2020, 1, 2)
        };
        var entity3 = new DateEntity
        {
            Property = new(2020, 1, 3)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Where_in_on_time()
    {
        var query =
            """
            {
              timeEntities (where: {property: {in: ["10:00:00"]}})
              {
                property
              }
            }
            """;

        var entity1 = new TimeEntity
        {
            Property = new(10, 0)
        };
        var entity2 = new TimeEntity
        {
            Property = new(11, 0)
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }

    // decimal had neither a list converter nor a Contains entry
    [Fact]
    public async Task Where_in_on_decimal()
    {
        var query =
            """
            {
              fieldBuilderProjectionEntities (where: {salary: {in: [10.5, 30]}})
              {
                name
                salary
              }
            }
            """;

        var entity1 = new FieldBuilderProjectionEntity
        {
            Name = "One",
            Salary = 10.5m
        };
        var entity2 = new FieldBuilderProjectionEntity
        {
            Name = "Two",
            Salary = 20
        };
        var entity3 = new FieldBuilderProjectionEntity
        {
            Name = "Three",
            Salary = 30
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2, entity3]);
    }
}
