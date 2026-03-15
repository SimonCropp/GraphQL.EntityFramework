public partial class IntegrationTests
{
    [Fact]
    public async Task Tph_derived_navigation_via_inline_fragments()
    {
        var category = new CategoryEntity { Name = "Science" };
        var region = new RegionEntity { Name = "North" };
        var categoryItem = new TphDerivedNavCategoryEntity
        {
            Property = "CategoryItem1",
            Category = category,
            CategoryId = category.Id
        };
        var regionItem = new TphDerivedNavRegionEntity
        {
            Property = "RegionItem1",
            Region = region,
            RegionId = region.Id
        };

        var query =
            """
            {
              tphDerivedNavEntities
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
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [category, region, categoryItem, regionItem]);
    }

    [Fact]
    public async Task Tph_derived_navigation_via_inline_fragments_single()
    {
        var category = new CategoryEntity { Name = "History" };
        var categoryItem = new TphDerivedNavCategoryEntity
        {
            Property = "CategoryItem2",
            Category = category,
            CategoryId = category.Id
        };

        var query = $$"""
            {
              tphDerivedNavEntity(id: "{{categoryItem.Id}}")
              {
                property
                ... on TphDerivedNavCategory {
                  category { name }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [category, categoryItem]);
    }

    [Fact]
    public async Task Tph_derived_navigation_base_fields_still_work()
    {
        var categoryItem = new TphDerivedNavCategoryEntity { Property = "BaseOnly" };
        var regionItem = new TphDerivedNavRegionEntity { Property = "BaseOnly2" };

        // Query only base fields - no inline fragments, no derived navigations needed
        var query =
            """
            {
              tphDerivedNavEntities
              {
                property
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [categoryItem, regionItem]);
    }

    [Fact]
    public async Task Tph_derived_navigation_mixed_with_base_scalars()
    {
        var category = new CategoryEntity { Name = "Art" };
        var region = new RegionEntity { Name = "South" };
        var categoryItem = new TphDerivedNavCategoryEntity
        {
            Property = "CatMixed",
            Category = category,
            CategoryId = category.Id
        };
        var regionItem = new TphDerivedNavRegionEntity
        {
            Property = "RegMixed",
            Region = region,
            RegionId = region.Id
        };

        // Query base scalar fields AND derived navigation fields together
        var query =
            """
            {
              tphDerivedNavEntities(orderBy: {path: "property"})
              {
                id
                property
                ... on TphDerivedNavCategory {
                  categoryId
                  category { name }
                }
                ... on TphDerivedNavRegion {
                  regionId
                  region { name }
                }
              }
            }
            """;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [category, region, categoryItem, regionItem]);
    }
}
