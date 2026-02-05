public partial class IntegrationTests
{
    [Fact]
    public async Task Abstract_root_type_query_falls_back_to_include()
    {
        var derivedEntity = new DerivedEntity
        {
            Property = "Derived1"
        };
        var child = new DerivedChildEntity
        {
            Property = "Child1",
            Parent = derivedEntity
        };
        derivedEntity.ChildrenFromBase.Add(child);

        var query =
            """
            {
              baseEntities
              {
                ... on Derived
                {
                  property
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [derivedEntity, child]);
    }

    [Fact]
    public async Task Navigation_to_abstract_type_falls_back_to_full_include()
    {
        var parent = new DerivedEntity
        {
            Property = "Parent1"
        };
        var child = new DerivedChildEntity
        {
            Property = "Child1",
            Parent = parent
        };

        var query =
            """
            {
              derivedChildEntities
              {
                property
                parent
                {
                  ... on Derived
                  {
                    property
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    [Fact]
    public async Task Readonly_root_with_navigation_falls_back_to_include()
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
              readOnlyEntities
              {
                firstName
                computedInDb
                readOnlyParent
                {
                  property
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }
}
