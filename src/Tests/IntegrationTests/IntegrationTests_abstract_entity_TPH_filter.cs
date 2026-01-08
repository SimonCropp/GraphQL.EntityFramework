public partial class IntegrationTests
{
    /// <summary>
    /// This test replicates the EXACT scenario from LegislationApi that causes the bug:
    /// 1. Query BaseEntities (abstract with TPH - like querying ProgramBillBase)
    /// 2. The query uses .Include() on a navigation property (like .Include(_ => _.Forecast))
    /// 3. Filters with projections are defined on the concrete derived types (DerivedEntity, DerivedWithNavigationEntity)
    ///
    /// Expected error: "Can't compile a NewExpression with a constructor declared on an abstract class"
    /// This happens because EF Core's query compilation for TPH + Include creates expressions referencing the abstract type.
    /// </summary>
    [Fact]
    public async Task Query_abstract_TPH_with_include_and_derived_type_filters()
    {
        // This query is analogous to: data.ProgramBills.Include(_ => _.Forecast).Where(...)
        var query =
            """
            {
              baseEntities(orderBy: {path: "property"}) {
                property
                childrenFromBase {
                  property
                }
              }
            }
            """;

        var derived1 = new DerivedEntity
        {
            Property = "Derived1"
        };
        var derived2 = new DerivedWithNavigationEntity
        {
            Property = "Derived2"
        };
        var child = new DerivedChildEntity
        {
            Property = "Child1",
            TypedParent = derived2
        };
        derived2.Children.Add(child);

        // Filters with projections on the concrete derived types
        // This is the key that triggers the bug when combined with TPH and navigation includes
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);
        filters.For<DerivedWithNavigationEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derived1, derived2, child]);
    }

    /// <summary>
    /// This test uses the actual BaseEntity abstract class with TPH inheritance.
    /// It queries through the childrenFromBase navigation which is typed as List&lt;DerivedChildEntity&gt;.
    /// This more closely matches the LegislationApi scenario.
    /// </summary>
    [Fact]
    public async Task Query_abstract_TPH_entity_with_filtered_navigation()
    {
        var query =
            """
            {
              baseEntities(orderBy: {path: "property"}) {
                property
                childrenFromBase {
                  property
                }
              }
            }
            """;

        var parent = new DerivedWithNavigationEntity
        {
            Property = "Parent1"
        };
        var child1 = new DerivedChildEntity
        {
            Property = "Child1",
            TypedParent = parent
        };
        var child2 = new DerivedChildEntity
        {
            Property = "Child2",
            TypedParent = parent
        };
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        // Filter with projection on DerivedChildEntity
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedChildEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child1, child2]);
    }

    /// <summary>
    /// Test with multiple filters on different derived types that inherit from same abstract base.
    /// This matches having filters on both ProgramBill and TreasuryProgramBill.
    /// </summary>
    [Fact]
    public async Task Query_abstract_TPH_with_filters_on_multiple_derived_types()
    {
        var query =
            """
            {
              baseEntities(orderBy: {path: "property"}) {
                property
                childrenFromBase {
                  property
                }
              }
            }
            """;

        var derived1 = new DerivedEntity
        {
            Property = "Derived1"
        };
        var derived2 = new DerivedWithNavigationEntity
        {
            Property = "Derived2"
        };
        var child = new DerivedChildEntity
        {
            Property = "Child1",
            TypedParent = derived2
        };
        derived2.Children.Add(child);

        // Filters on both derived types - matches ProgramBill and TreasuryProgramBill filters
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);
        filters.For<DerivedWithNavigationEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);
        filters.For<DerivedChildEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derived1, derived2, child]);
    }
}
