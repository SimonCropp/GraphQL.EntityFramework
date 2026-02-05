public partial class IntegrationTests
{
    [Fact]
    public async Task ReadOnly_circular_include_does_not_walkback()
    {
        var parent = new ReadOnlyParentEntity
        {
            Property = "TheParent"
        };
        var child = new ReadOnlyEntity
        {
            FirstName = "John",
            LastName = "Smith",
            Age = 25,
            ReadOnlyParent = parent,
            ReadOnlyParentId = parent.Id
        };

        var query =
            """
            {
              readOnlyEntities {
                firstName
                displayName
                readOnlyParent {
                  property
                  children {
                    firstName
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    [Fact]
    public async Task Single_child_to_parent_walkback_skipped()
    {
        var entity1 = new ParentEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Child1",
            Parent = entity1
        };

        var query =
            """
            {
              parentEntity(id: "00000000-0000-0000-0000-000000000001") {
                property
                children {
                  property
                  parent {
                    property
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entity1, entity2]);
    }
}
