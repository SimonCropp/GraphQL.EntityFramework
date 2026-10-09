public partial class IntegrationTests
{
    // An abstract type is projected as a chain of type tests, each creating a concrete derived
    // type. It used to be loaded whole through includes.
    [Fact]
    public async Task Abstract_root_type_query_is_projected()
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

    // A collection selected on a type with derived types is bound once, to whichever type the
    // chain of type tests created. Bound inside each type's member init it was joined once per
    // concrete type.
    [Fact]
    public async Task Abstract_root_type_collection_is_loaded_once()
    {
        var parent = new DerivedEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Parent1"
        };
        var child1 = new DerivedChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Child1",
            Parent = parent
        };
        var child2 = new DerivedChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Child2",
            Parent = parent
        };
        var other = new DerivedWithNavigationEntity
        {
            Id = new("00000000-0000-0000-0000-000000000004"),
            Property = "Parent2"
        };

        var query =
            """
            {
              baseEntities(orderBy: {property: ascending})
              {
                property
                childrenFromInterface(orderBy: {property: ascending})
                {
                  items
                  {
                    property
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2, other]);
    }

    [Fact]
    public async Task Navigation_to_abstract_type_is_projected()
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

    // displayName is expression bodied, so the select cannot bind it and the query falls back to
    // includes. A mapped get only property, such as computedInDb, no longer causes this.
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
                displayName
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
