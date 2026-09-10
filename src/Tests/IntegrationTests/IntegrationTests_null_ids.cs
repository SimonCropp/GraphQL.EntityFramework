public partial class IntegrationTests
{
    // A null ids argument was dereferenced for its type name while building the unsupported
    // type error. It is now the same as not passing ids.
    [Fact]
    public async Task Null_ids_returns_everything()
    {
        var query =
            """
            query($ids: [ID!])
            {
              parentEntities(ids: $ids, orderBy: {property: ascending})
              {
                property
              }
            }
            """;

        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        var inputs = new Inputs(new Dictionary<string, object?>
        {
            ["ids"] = null
        });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, [entity1, entity2]);
    }
}
