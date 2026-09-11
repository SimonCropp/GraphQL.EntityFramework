public partial class IntegrationTests
{
    // The resolver-less navigation overloads on an object type fell back to GraphQL.NET's name
    // resolver, which returned the raw property, ignored the arguments and applied no filters.
    // They now get the same resolver AutoMap uses.
    [Fact]
    public async Task Resolverless_list_applies_arguments_and_filters()
    {
        var query =
            """
            {
              parentEntities
              {
                childrenNoResolve(where: {property: {startsWith: "Value"}}, orderBy: property_desc)
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
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };
        var child3 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000003"),
            Property = "Other",
            Parent = parent
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<ChildEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "Value2");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child1, child2, child3]);
    }

    [Fact]
    public async Task Resolverless_connection_pages()
    {
        var query =
            """
            {
              parentEntities
              {
                childrenConnectionNoResolve(first: 1, orderBy: property)
                {
                  totalCount
                  items
                  {
                    property
                  }
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Value1",
            Parent = parent
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = parent
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }

    [Fact]
    public async Task Resolverless_single_applies_filters()
    {
        var query =
            """
            {
              childEntities(orderBy: property)
              {
                property
                parentNoResolve
                {
                  property
                }
              }
            }
            """;

        var allowed = new ParentEntity
        {
            Property = "Allowed"
        };
        var denied = new ParentEntity
        {
            Property = "Denied"
        };
        var child1 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000001"),
            Property = "Child1",
            Parent = allowed
        };
        var child2 = new ChildEntity
        {
            Id = new("00000000-0000-0000-0000-000000000002"),
            Property = "Child2",
            Parent = denied
        };

        var filters = new Filters<IntegrationDbContext>();
        filters.For<ParentEntity>().Add(
            projection: _ => _.Property,
            filter: (_, _, _, property) => property != "Denied");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [allowed, denied, child1, child2]);
    }

    // The field's selection set went to the first path in the projection expression, so with
    // `new { _.Child2, _.Child1 }` resolving Child1, nothing under Child1 was projected
    [Fact]
    public async Task Selection_applies_to_navigation_of_field_type()
    {
        var query =
            """
            {
              manyChildren
              {
                child1Second
                {
                  id
                  parent
                  {
                    id
                  }
                }
              }
            }
            """;

        var parent = new WithManyChildrenEntity();
        var child1 = new Child1Entity
        {
            Parent = parent
        };
        var child2 = new Child2Entity
        {
            Parent = parent
        };
        parent.Child1 = child1;
        parent.Child2 = child2;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child1, child2]);
    }
}
