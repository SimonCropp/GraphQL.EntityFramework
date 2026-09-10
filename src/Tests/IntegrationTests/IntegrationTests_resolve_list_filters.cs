public partial class IntegrationTests
{
    // ResolveList and ResolveListAsync returned their items as is, while every other list path
    // applies the filters, so a filter that excluded an item elsewhere let it through here
    [Theory]
    [InlineData("childrenViaResolveList")]
    [InlineData("childrenViaResolveListAsync")]
    public async Task Resolve_list_applies_filters(string fieldName)
    {
        var query =
            $$"""
            {
              parentEntities
              {
                {{fieldName}}
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var allowed = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Allowed",
            Parent = parent
        };
        var denied = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Denied",
            Parent = parent
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<ChildEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "Denied");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, allowed, denied]);
    }
}
