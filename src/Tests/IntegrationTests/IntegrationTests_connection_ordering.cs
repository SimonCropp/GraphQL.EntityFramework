public partial class IntegrationTests
{
    [Fact]
    public async Task Connection_with_read_only_property_and_navigation()
    {
        // ReadOnlyEntity has read-only properties (DisplayName, IsAdult, ComputedInDb)
        // which cause SelectExpressionBuilder to bail (returns null).
        // This forces the AddIncludes path, where Include() strips IOrderedQueryable.
        // The ordering check in ConnectionConverter should still pass by inspecting
        // the expression tree for OrderBy/OrderByDescending.
        var query =
            """
            {
              readOnlyEntitiesConnection(first:2) {
                totalCount
                items {
                  firstName
                  computedInDb
                  readOnlyParent {
                    property
                  }
                }
              }
            }
            """;

        var parent = new ReadOnlyParentEntity { Property = "Parent1" };
        var entity1 = new ReadOnlyEntity { FirstName = "Alice", LastName = "A", Age = 25, ReadOnlyParent = parent };
        var entity2 = new ReadOnlyEntity { FirstName = "Bob", LastName = "B", Age = 17, ReadOnlyParent = parent };
        var entity3 = new ReadOnlyEntity { FirstName = "Charlie", LastName = "C", Age = 30, ReadOnlyParent = parent };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, entity1, entity2, entity3]);
    }

    [Fact]
    public async Task Connection_with_read_only_property_navigation_and_fragment()
    {
        var query =
            """
            {
              readOnlyEntitiesConnection(first:2) {
                totalCount
                items {
                  ...readOnlyFields
                }
              }
            }

            fragment readOnlyFields on ReadOnlyEntity {
              firstName
              computedInDb
              readOnlyParent {
                property
              }
            }
            """;

        var parent = new ReadOnlyParentEntity { Property = "Parent1" };
        var entity1 = new ReadOnlyEntity { FirstName = "Alice", LastName = "A", Age = 25, ReadOnlyParent = parent };
        var entity2 = new ReadOnlyEntity { FirstName = "Bob", LastName = "B", Age = 17, ReadOnlyParent = parent };
        var entity3 = new ReadOnlyEntity { FirstName = "Charlie", LastName = "C", Age = 30, ReadOnlyParent = parent };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, entity1, entity2, entity3]);
    }
}
