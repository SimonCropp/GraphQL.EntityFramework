public partial class IntegrationTests
{
    // Tests that AutoMap'd collection navigation fields apply filters correctly
    // FilterParentGraphType uses AutoMap() which registers the Children navigation
    [Fact]
    public async Task AutoMap_ListNavigation_AppliesFilters()
    {
        var query =
            """
            {
              parentEntitiesFiltered
              {
                property
                children
                {
                  property
                }
              }
            }
            """;

        var parent = new FilterParentEntity
        {
            Property = "ParentValue"
        };
        var childIgnored = new FilterChildEntity
        {
            Property = "Ignore", // Should be filtered out
            Parent = parent
        };
        var childKept = new FilterChildEntity
        {
            Property = "KeepMe",
            Parent = parent
        };
        parent.Children.Add(childIgnored);
        parent.Children.Add(childKept);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [parent, childIgnored, childKept]);
    }

    // Tests that projection-based AddNavigationListField applies filters
    // This uses the simple projection-based overload: AddNavigationListField(graph, name, projection)
    [Fact]
    public async Task ProjectionBased_NavigationList_AppliesFilters()
    {
        var query =
            """
            {
              parentEntitiesFiltered
              {
                property
                children
                {
                  property
                }
              }
            }
            """;

        var parent1 = new FilterParentEntity
        {
            Property = "Parent1"
        };
        var parent2 = new FilterParentEntity
        {
            Property = "Ignore" // Parent should be filtered out
        };
        var child1 = new FilterChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        var child2 = new FilterChildEntity
        {
            Property = "Ignore", // Child should be filtered out
            Parent = parent1
        };
        parent1.Children.Add(child1);
        parent1.Children.Add(child2);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [parent1, parent2, child1, child2]);
    }

    // Tests that projection-based AddNavigationField applies ShouldInclude filter
    // Multiple_nested_Filtered already tests this scenario with Level3Entity
    [Fact]
    public async Task ProjectionBased_NavigationSingle_AppliesFilters()
    {
        var query =
            """
            {
              level1Entities
              {
                level2Entity
                {
                  level3Entity
                  {
                    property
                  }
                }
              }
            }
            """;

        var level3Ignored = new Level3Entity
        {
            Property = "Ignore" // Should be filtered to null
        };
        var level2 = new Level2Entity
        {
            Level3Entity = level3Ignored
        };
        var level1 = new Level1Entity
        {
            Level2Entity = level2
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, BuildFilters(), false, [level1, level2, level3Ignored]);
    }

}
