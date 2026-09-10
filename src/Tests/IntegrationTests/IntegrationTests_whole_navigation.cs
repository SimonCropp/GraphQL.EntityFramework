public partial class IntegrationTests
{
    // A filter whose projection is the navigation itself had that navigation skipped when merging
    // its requirements, so the filter saw null. It is now loaded whole.
    [Fact]
    public async Task Filter_reading_whole_navigation_sees_its_properties()
    {
        var allowParent = new ParentEntity
        {
            Property = "Allow"
        };
        var denyParent = new ParentEntity
        {
            Property = "Deny"
        };
        var allowed = new ChildEntity
        {
            Property = "Allowed",
            Parent = allowParent
        };
        var denied = new ChildEntity
        {
            Property = "Denied",
            Parent = denyParent
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<ChildEntity>().Add(
            projection: _ => _.Parent,
            filter: (_, _, _, parent) => parent?.Property != "Deny");

        var query =
            """
            {
              childEntities(orderBy: {path: "property"})
              {
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [allowParent, denyParent, allowed, denied]);
    }
}
