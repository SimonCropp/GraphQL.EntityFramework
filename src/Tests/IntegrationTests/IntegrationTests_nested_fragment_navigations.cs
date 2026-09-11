public partial class IntegrationTests
{
    // Derived type navigations under a fragment were collected for the root selection only, so
    // below a navigation the derived navigation fell through to the scalar path and was dropped
    [Fact]
    public async Task Derived_navigation_under_fragment_below_reference_navigation()
    {
        var category = new CategoryEntity
        {
            Name = "Science"
        };
        var item = new TphDerivedNavCategoryEntity
        {
            Property = "CategoryItem",
            Category = category
        };
        var owner = new TphDerivedNavOwnerEntity
        {
            Property = "Owner",
            Item = item
        };

        var query =
            """
            {
              tphDerivedNavOwners
              {
                property
                item
                {
                  property
                  ... on TphDerivedNavCategory {
                    category { name }
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [category, item, owner]);
    }

    [Fact]
    public async Task Derived_navigations_under_fragments_below_collection_navigation()
    {
        var category = new CategoryEntity
        {
            Name = "Science"
        };
        var region = new RegionEntity
        {
            Name = "North"
        };
        var owner = new TphDerivedNavOwnerEntity
        {
            Property = "Owner",
            Items =
            [
                new TphDerivedNavCategoryEntity
                {
                    Property = "CategoryItem",
                    Category = category
                },
                new TphDerivedNavRegionEntity
                {
                    Property = "RegionItem",
                    Region = region
                }
            ]
        };

        var query =
            """
            {
              tphDerivedNavOwners
              {
                property
                items (orderBy: property)
                {
                  property
                  ... on TphDerivedNavCategory {
                    category { name }
                  }
                  ... on TphDerivedNavRegion {
                    region { name }
                  }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [category, region, owner]);
    }
}
