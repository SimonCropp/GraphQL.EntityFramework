// A navigation list without an orderBy is ordered by key by default. With orderNavigationsByKey
// off it is left unordered, so an ordering the model applies to the collections a projection
// loads, such as EfOrderBy's default ordering, is used instead. ChildEntity defaults to Property.
public partial class IntegrationTests
{
    const string parentChildQuery =
        """
        {
          parentEntities
          {
            property
            children
            {
              property
            }
          }
        }
        """;

    static object[] ParentWithChild()
    {
        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child = new ChildEntity
        {
            Property = "Child",
            Parent = parent
        };
        parent.Children.Add(child);
        return [parent, child];
    }

    [Fact]
    public async Task Navigation_without_orderBy_ordered_by_key()
    {
        await using var database = await sqlInstance.Build();
        await RunQuery(database, parentChildQuery, null, null, false, ParentWithChild());
    }

    [Fact]
    public async Task Navigation_without_orderBy_not_ordered_by_key()
    {
        await using var database = await sqlInstance.Build();
        await RunQuery(database, parentChildQuery, null, null, false, ParentWithChild(), orderNavigationsByKey: false);
    }

    [Fact]
    public async Task Navigation_with_orderBy_not_ordered_by_key()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                children(orderBy: {property: descending})
                {
                  property
                }
              }
            }
            """;
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, ParentWithChild(), orderNavigationsByKey: false);
    }
}
