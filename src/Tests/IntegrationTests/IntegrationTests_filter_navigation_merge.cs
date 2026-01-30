public partial class IntegrationTests
{
    /// <summary>
    /// Tests that filter-required navigation properties are merged with GraphQL-requested fields.
    /// When a filter accesses Parent.Property and the GraphQL query requests Parent.Id,
    /// both should be selected in the SQL query.
    /// </summary>
    [Fact]
    public async Task Filter_navigation_properties_merged_with_graphql_fields()
    {
        var query =
            """
            {
              childEntities
              {
                property
                parent
                {
                  id
                }
              }
            }
            """;

        var parent1 = new ParentEntity { Property = "Parent1" };
        var parent2 = new ParentEntity { Property = "Parent2" };

        var entity1 = new ChildEntity { Property = "Child1", Parent = parent1 };
        var entity2 = new ChildEntity { Property = "Child2", Parent = parent2 };
        var entity3 = new ChildEntity { Property = "Child3", Parent = parent1 };

        var filters = new Filters<IntegrationDbContext>();

        // Filter accesses Parent.Property (not requested in GraphQL)
        // GraphQL requests Parent.Id
        // Both should be in the final query
        filters.For<ChildEntity>().Add(
            projection: c => new ChildFilterInfo(
                c.Parent != null ? c.Parent.Property : null,
                c.Id),
            filter: (_, _, _, p) => p.ParentProperty == "Parent1");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent1, parent2, entity1, entity2, entity3]);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    record ChildFilterInfo(string? ParentProperty, Guid ChildId);
}
