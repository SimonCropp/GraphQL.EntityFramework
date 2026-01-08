public partial class IntegrationTests
{
    [Fact]
    public async Task Filter_on_abstract_base_entity_with_id_projection()
    {
        var query =
            """
            {
              baseEntities(orderBy: {path: "property"}) {
                property
              }
            }
            """;

        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();
        var derivedEntity1 = new DerivedEntity
        {
            Id = entity1Id,
            Property = "Value1"
        };
        var derivedEntity2 = new DerivedWithNavigationEntity
        {
            Id = entity2Id,
            Property = "Value2"
        };

        // Filter with Id projection on entities that inherit from abstract class
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);
        filters.For<DerivedWithNavigationEntity>().Add(
            projection: _ => _.Id,
            filter: (_, _, _, id) => id != Guid.Empty);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derivedEntity1, derivedEntity2]);
    }

    [Fact]
    public async Task Filter_on_derived_entity_with_navigation_and_projection()
    {
        var query =
            """
            {
              baseEntities(orderBy: {path: "property"}) {
                property
                ... on DerivedWithNavigationEntity {
                  children {
                    property
                  }
                }
              }
            }
            """;

        var derivedEntity1 = new DerivedWithNavigationEntity
        {
            Property = "Parent1"
        };
        var child1 = new DerivedChildEntity
        {
            Property = "Child1",
            TypedParent = derivedEntity1
        };
        derivedEntity1.Children.Add(child1);

        var derivedEntity2 = new DerivedEntity
        {
            Property = "Value2"
        };

        // Filter with projection on entity that inherits from abstract class
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedWithNavigationEntity>().Add(
            projection: _ => _.Property!,
            filter: (_, _, _, property) => property != "IgnoreMe");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derivedEntity1, derivedEntity2, child1]);
    }

    [Fact]
    public async Task Filter_on_child_entity_referencing_abstract_parent_with_projection()
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

        var derivedParent = new DerivedWithNavigationEntity
        {
            Property = "Parent1"
        };
        var child1 = new DerivedChildEntity
        {
            Property = "KeepMe",
            TypedParent = derivedParent
        };
        var child2 = new DerivedChildEntity
        {
            Property = "IgnoreMe",
            TypedParent = derivedParent
        };
        derivedParent.Children.Add(child1);
        derivedParent.Children.Add(child2);

        // Filter on child entity that has a navigation property to abstract parent
        var filters = new Filters<IntegrationDbContext>();
        filters.For<DerivedChildEntity>().Add(
            projection: _ => _.Property!,
            filter: (_, _, _, property) => property != "IgnoreMe");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [derivedParent, child1, child2]);
    }
}
