public partial class IntegrationTests
{
    /// <summary>
    /// Setup:
    /// - BaseEntity is abstract with TPH inheritance (Discriminator)
    /// - DerivedChildEntity has navigation property Parent that references BaseEntity (abstract)
    /// - Filters with projections are defined on the concrete derived types (DerivedEntity, DerivedWithNavigationEntity)
    ///
    /// Bug:
    /// When querying DerivedChildEntity and the query includes parent (navigation),
    /// GraphQL.EntityFramework tries to build projection for the Parent navigation.
    /// Since Parent is typed as BaseEntity (abstract), SelectExpressionBuilder tries to create:
    ///     Expression.New(typeof(BaseEntity))
    /// Which fails with: "Can't compile a NewExpression with a constructor declared on an abstract class"
    ///
    /// Fix:
    /// SelectExpressionBuilder now checks if navType.IsAbstract and returns null to skip projection.
    ///
    /// This test would FAIL before the fix and PASS after.
    /// </summary>
    [Fact]
    public async Task Query_with_navigation_to_abstract_base_and_derived_filters_with_projection()
    {
        // Query that includes navigation back to abstract parent
        // This triggers projection of Parent which is BaseEntity (abstract)
        var query =
            """
            {
              derivedChildEntities {
                property
                parent {
                  property
                }
              }
            }
            """;

        var parent1 = new DerivedEntity
        {
            Property = "Parent1"
        };
        var parent2 = new DerivedWithNavigationEntity
        {
            Property = "Parent2"
        };
        var child1 = new DerivedChildEntity
        {
            Property = "Child1",
            Parent = parent1
        };
        var child2 = new DerivedChildEntity
        {
            Property = "Child2",
            Parent = parent2
        };

        // KEY: Filters with projections on the concrete derived types
        // When the query tries to project Parent (which is BaseEntity abstract),
        // it will try to apply these filters and build projection expressions
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);
        filters.For<DerivedWithNavigationEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        // Before fix: This would throw "Can't compile a NewExpression with a constructor declared on an abstract class"
        // After fix: This should work (skips projection for abstract navigation, loads full entity)
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent1, parent2, child1, child2]);
    }

    /// <summary>
    /// Simpler version: Query with abstract parent but don't include it in projection.
    /// This tests that the filter system works even when Parent is abstract.
    /// </summary>
    [Fact]
    public async Task Query_entity_with_abstract_parent_navigation_and_filters()
    {
        var query =
            """
            {
              derivedChildEntities {
                property
              }
            }
            """;

        var parent = new DerivedEntity
        {
            Property = "Parent"
        };
        var child = new DerivedChildEntity
        {
            Property = "Child",
            Parent = parent
        };

        // Filters on abstract derived types with projections
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Property!,
            filter: (_, _, _, prop) => prop != "Skip");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child]);
    }
}
