public partial class IntegrationTests
{
    /// <summary>
    /// This test replicates the scenario where:
    /// 1. We query an entity (like SittingWeek) that has no filter
    /// 2. That entity has a navigation property to entities that inherit from an abstract class (like ProgramBill : ProgramBillBase)
    /// 3. The derived entities have filters with projections
    /// This should trigger the "Can't compile a NewExpression with a constructor declared on an abstract class" error
    /// </summary>
    [Fact]
    public async Task Query_navigation_to_filtered_abstract_derived_entities()
    {
        var query =
            """
            {
              parentEntities {
                property
                children {
                  property
                }
              }
            }
            """;

        var parent = new ParentEntity
        {
            Property = "Parent1"
        };
        var child1 = new ChildEntity
        {
            Property = "Child1",
            Parent = parent
        };
        parent.Children.Add(child1);

        // Apply filters to child entities (simulating ProgramBill having a filter)
        var filters = new Filters<IntegrationDbContext>();
        filters.For<ChildEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child1]);
    }

    /// <summary>
    /// This test uses entities that actually inherit from an abstract base (BaseEntity)
    /// and adds a filter with projection to them, then queries through navigation
    /// </summary>
    [Fact]
    public async Task Query_navigation_to_filtered_entities_inheriting_from_abstract()
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

        var parentId = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();

        var parent = new DerivedWithNavigationEntity
        {
            Id = parentId,
            Property = "Parent1"
        };
        var child1 = new DerivedChildEntity
        {
            Id = child1Id,
            Property = "Child1",
            TypedParent = parent
        };
        var child2 = new DerivedChildEntity
        {
            Id = child2Id,
            Property = "Child2",
            TypedParent = parent
        };
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        // Add filter with projection to both parent and child entities that inherit from abstract class
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedWithNavigationEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);
        filters.For<DerivedChildEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child1, child2]);
    }
}
