public partial class IntegrationTests
{
    /// <summary>
    /// Tests that calling AddIncludes on a projected query (one that has Select applied)
    /// gracefully skips adding Include rather than throwing.
    /// This tests the IsProjectedQuery check in IncludeAppender.AddIncludes.
    ///
    /// When only scalar fields are requested, the query executes successfully.
    /// </summary>
    [Fact]
    public async Task AddIncludes_on_projected_query_without_navigation_request()
    {
        // Query only scalar fields - the projected query executes successfully.
        // The AddIncludes call detects the query is projected and returns early
        // without attempting to add Include.
        var query =
            """
            {
              queryFieldWithManualAddIncludes {
                id
              }
            }
            """;

        var entityA = new IncludeNonQueryableA();
        var entityB = new IncludeNonQueryableB
        {
            IncludeNonQueryableA = entityA,
            IncludeNonQueryableAId = entityA.Id
        };
        entityA.IncludeNonQueryableB = entityB;
        entityA.IncludeNonQueryableBId = entityB.Id;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entityA, entityB]);
    }

    /// <summary>
    /// Tests that calling AddIncludes on a projected query skips Include,
    /// which means navigation properties won't be loaded. When the GraphQL query
    /// requests a non-nullable navigation field, this results in a null error
    /// because the navigation data wasn't loaded.
    ///
    /// This verifies that:
    /// 1. The Include attempt is skipped (no EF "Include after Select" error)
    /// 2. The query executes (SQL is generated and run)
    /// 3. GraphQL properly reports the null navigation field error
    /// </summary>
    [Fact]
    public async Task AddIncludes_on_projected_query_with_navigation_returns_null()
    {
        // Request a navigation field. Since Include is skipped on projected queries,
        // the navigation property will be null, causing a GraphQL null error.
        var query =
            """
            {
              queryFieldWithManualAddIncludes {
                id
                includeNonQueryableB {
                  id
                }
              }
            }
            """;

        var entityA = new IncludeNonQueryableA();
        var entityB = new IncludeNonQueryableB
        {
            IncludeNonQueryableA = entityA,
            IncludeNonQueryableAId = entityA.Id
        };
        entityA.IncludeNonQueryableB = entityB;
        entityA.IncludeNonQueryableBId = entityB.Id;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, [entityA, entityB]);
    }
}
