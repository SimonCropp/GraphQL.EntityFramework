public partial class IntegrationTests
{
    // Below the root, fragments were followed one level down only, so a fragment that spread
    // another fragment contributed nothing to the projection.
    [Fact]
    public async Task Fragment_spread_inside_fragment_spread()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                children
                {
                  ...childFields
                }
              }
            }

            fragment childFields on Child
            {
              ...childProperty
            }

            fragment childProperty on Child
            {
              property
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

    [Fact]
    public async Task Fragment_spread_inside_inline_fragment()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                children
                {
                  ... on Child
                  {
                    ...childProperty
                  }
                }
              }
            }

            fragment childProperty on Child
            {
              property
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

    // The root is abstract, so this takes the include path. The navigation on the derived type is
    // selected by an inline fragment that sits inside a fragment spread, which was not followed.
    [Fact]
    public async Task Derived_navigation_inline_fragment_inside_fragment_spread()
    {
        var category = new CategoryEntity
        {
            Name = "Science"
        };
        var categoryItem = new TphDerivedNavCategoryEntity
        {
            Property = "CategoryItem1",
            Category = category,
            CategoryId = category.Id
        };
        var regionItem = new TphDerivedNavRegionEntity
        {
            Property = "RegionItem1"
        };

        var query =
            """
            {
              tphDerivedNavEntities
              {
                ...baseFields
              }
            }

            fragment baseFields on TphDerivedNavBaseEntity
            {
              property
              ... on TphDerivedNavCategory
              {
                category
                {
                  name
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [category, categoryItem, regionItem]);
    }
}
