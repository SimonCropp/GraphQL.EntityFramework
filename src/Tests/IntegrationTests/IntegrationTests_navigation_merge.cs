public partial class IntegrationTests
{
    // The same navigation selected under two aliases. Whichever selection was processed first
    // used to win, so the fields asked for by the other came back as defaults.
    [Fact]
    public async Task Aliased_navigation_selections_merge()
    {
        var query =
            """
            {
              employees
              {
                name
                a: department
                {
                  name
                }
                b: department
                {
                  isActive
                }
              }
            }
            """;

        var department = new DepartmentEntity
        {
            Name = "Engineering",
            IsActive = true
        };
        var employee = new EmployeeEntity
        {
            Name = "Alice",
            Department = department
        };
        department.Employees.Add(employee);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [department, employee]);
    }

    // A navigation field and a projection based field that projects the same navigation, selected
    // side by side. The navigation field was processed first, and the projection based field was
    // then skipped, so the fields it selected were never projected.
    [Fact]
    public async Task Projection_based_field_merges_with_navigation_field()
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
                parentAlias
                {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        var child = new ChildEntity
        {
            Property = "Value2",
            Parent = parent
        };
        parent.Children.Add(child);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }

    // Navigations below the aliased selections are merged as well
    [Fact]
    public async Task Aliased_navigation_selections_merge_nested_navigations()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                a: children
                {
                  property
                }
                b: children
                {
                  parentAlias
                  {
                    property
                  }
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Value1"
        };
        var child = new ChildEntity
        {
            Property = "Value2",
            Parent = parent
        };
        parent.Children.Add(child);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [parent, child]);
    }
}
