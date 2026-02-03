/// <summary>
/// Tests for the fix where filter projections accessing properties through abstract navigation types
/// should use Include as a fallback when projection can't be built (because you can't do "new AbstractType { ... }").
///
/// Bug scenario eg:
/// - Aircraft has TravelRequest navigation (TravelRequest extends abstract BaseRequest)
/// - Filter projection: _ => new RequestInfo(_.TravelRequest.GroupOwnerId, _.TravelRequest.HighestStatusAchieved, ...)
/// - SelectExpressionBuilder can't build projection for abstract TravelRequest type
/// - Without Include fallback, TravelRequest is never loaded → properties are always null
/// - Fix: Add Include for filter-required abstract navigations as fallback
/// </summary>
public partial class IntegrationTests
{
    /// <summary>
    /// Tests that filter projections accessing properties through abstract navigation types
    /// correctly load the navigation data via Include fallback.
    ///
    /// Setup:
    /// - DerivedChildEntity has Parent navigation of type BaseEntity (abstract)
    /// - Filter projection accesses Parent.Status (like HighestStatusAchieved in MinistersApi)
    /// - Filter checks if Parent.Status is a specific value
    ///
    /// Expected: Entity is found because Include loads Parent, allowing filter to evaluate correctly
    /// Bug (before fix): Parent.Status is null because projection fails and no Include fallback
    /// </summary>
    [Fact]
    public async Task Filter_with_abstract_navigation_should_use_include_fallback()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                id
                property
              }
            }
            """;

        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        // Create parent with specific Status value
        var parent = new DerivedEntity
        {
            Id = parentId,
            Property = "Parent1",
            Status = "Approved" // This is the value we'll filter on
        };

        // Create child that references the parent via abstract BaseEntity navigation
        var child = new DerivedChildEntity
        {
            Id = childId,
            Property = "Child1",
            ParentId = parentId,
            Parent = parent
        };

        var filters = new Filters<IntegrationDbContext>();

        // Filter projection accesses Status through abstract Parent navigation
        // This mirrors the scenario:
        // _.TravelRequest != null ? _.TravelRequest.HighestStatusAchieved : null
        filters.For<DerivedChildEntity>().Add(
            projection: c => new AbstractNavStatusProjection(
                c.Parent != null ? c.Parent.Status : null,
                c.Id),
            filter: (_, _, _, proj) => proj.ParentStatus == "Approved");

        await using var database = await sqlInstance.Build();

        // Before fix: Child would NOT be returned because Parent.Status is null (Include not added)
        // After fix: Child IS returned because Include loads Parent, making Status available
        await RunQuery(database, query, null, filters, false, [parent, child]);
    }

    /// <summary>
    /// Tests that filter correctly EXCLUDES entities when abstract navigation property
    /// doesn't match the filter criteria.
    ///
    /// This verifies the Include fallback is working - if Status wasn't loaded,
    /// all entities would be excluded (Status would be null, never matching "Approved").
    /// </summary>
    [Fact]
    public async Task Filter_with_abstract_navigation_should_correctly_exclude_non_matching()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                id
                property
              }
            }
            """;

        var parent1Id = Guid.NewGuid();
        var parent2Id = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();

        // Parent with Status that MATCHES filter
        var parent1 = new DerivedEntity
        {
            Id = parent1Id,
            Property = "Parent1",
            Status = "Approved"
        };

        // Parent with Status that does NOT match filter
        var parent2 = new DerivedEntity
        {
            Id = parent2Id,
            Property = "Parent2",
            Status = "Draft" // This should be excluded
        };

        var child1 = new DerivedChildEntity
        {
            Id = child1Id,
            Property = "Child1-Approved",
            ParentId = parent1Id,
            Parent = parent1
        };

        var child2 = new DerivedChildEntity
        {
            Id = child2Id,
            Property = "Child2-Draft",
            ParentId = parent2Id,
            Parent = parent2
        };

        var filters = new Filters<IntegrationDbContext>();

        // Filter should only allow entities where Parent.Status == "Approved"
        filters.For<DerivedChildEntity>().Add(
            projection: c => new AbstractNavStatusProjection(
                c.Parent != null ? c.Parent.Status : null,
                c.Id),
            filter: (_, _, _, proj) => proj.ParentStatus == "Approved");

        await using var database = await sqlInstance.Build();

        // Only child1 should be returned (parent has Status "Approved")
        // child2 should be filtered out (parent has Status "Draft")
        // Verified via snapshot - should only contain Child1-Approved
        await RunQuery(database, query, null, filters, false, [parent1, parent2, child1, child2]);
    }

    /// <summary>
    /// Tests that filter projection with multiple properties from abstract navigation works.
    /// This matches the scenario more closely where both GroupOwnerId and
    /// HighestStatusAchieved are accessed from the abstract TravelRequest navigation.
    /// </summary>
    [Fact]
    public async Task Filter_with_multiple_properties_from_abstract_navigation()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                id
                property
              }
            }
            """;

        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        var parent = new DerivedEntity
        {
            Id = parentId,
            Property = "TargetProperty",
            Status = "Approved"
        };

        var child = new DerivedChildEntity
        {
            Id = childId,
            Property = "Child1",
            ParentId = parentId,
            Parent = parent
        };

        var filters = new Filters<IntegrationDbContext>();

        // Access multiple properties from abstract Parent navigation
        // (like accessing both GroupOwnerId and HighestStatusAchieved in MinistersApi)
        filters.For<DerivedChildEntity>().Add(
            projection: c => new AbstractNavMultiPropertyProjection(
                c.Parent != null ? c.Parent.Property : null,
                c.Parent != null ? c.Parent.Status : null,
                c.Id),
            filter: (_, _, _, proj) =>
                proj is { ParentProperty: "TargetProperty", ParentStatus: "Approved" });

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [parent, child]);
    }

    /// <summary>
    /// Tests that null parent navigation is handled correctly.
    /// The filter should handle cases where the navigation is null.
    /// </summary>
    [Fact]
    public async Task Filter_with_null_abstract_navigation_should_handle_gracefully()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                id
                property
              }
            }
            """;

        var childWithParentId = Guid.NewGuid();
        var childWithoutParentId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var parent = new DerivedEntity
        {
            Id = parentId,
            Property = "Parent1",
            Status = "Approved"
        };

        var childWithParent = new DerivedChildEntity
        {
            Id = childWithParentId,
            Property = "ChildWithParent",
            ParentId = parentId,
            Parent = parent
        };

        // Child with null Parent - should be excluded by filter (null status != "Approved")
        var childWithoutParent = new DerivedChildEntity
        {
            Id = childWithoutParentId,
            Property = "ChildWithoutParent",
            ParentId = null,
            Parent = null
        };

        var filters = new Filters<IntegrationDbContext>();

        filters.For<DerivedChildEntity>().Add(
            projection: c => new AbstractNavStatusProjection(
                c.Parent != null ? c.Parent.Status : null,
                c.Id),
            filter: (_, _, _, proj) => proj.ParentStatus == "Approved");

        await using var database = await sqlInstance.Build();

        // Only childWithParent should be returned
        // Verified via snapshot - childWithoutParent should be filtered out (null Parent)
        await RunQuery(database, query, null, filters, false, [parent, childWithParent, childWithoutParent]);
    }

    /// <summary>
    /// Tests combination of abstract navigation filter with concrete navigation filter.
    /// DerivedChildEntity has both:
    /// - Parent (abstract BaseEntity)
    /// - TypedParent (concrete DerivedWithNavigationEntity)
    /// </summary>
    [Fact]
    public async Task Filter_combining_abstract_and_concrete_navigations()
    {
        var query =
            """
            {
              derivedChildEntities
              {
                id
                property
              }
            }
            """;

        var abstractParentId = Guid.NewGuid();
        var concreteParentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        // Abstract parent (DerivedEntity extends BaseEntity)
        var abstractParent = new DerivedEntity
        {
            Id = abstractParentId,
            Property = "AbstractParent",
            Status = "Approved"
        };

        // Concrete parent (DerivedWithNavigationEntity is concrete)
        var concreteParent = new DerivedWithNavigationEntity
        {
            Id = concreteParentId,
            Property = "ConcreteParent"
        };

        var child = new DerivedChildEntity
        {
            Id = childId,
            Property = "Child1",
            ParentId = abstractParentId,
            Parent = abstractParent,
            TypedParentId = concreteParentId,
            TypedParent = concreteParent
        };

        var filters = new Filters<IntegrationDbContext>();

        // Filter on abstract navigation (should use Include fallback)
        filters.For<DerivedChildEntity>().Add(
            projection: c => new AbstractNavStatusProjection(
                c.Parent != null ? c.Parent.Status : null,
                c.Id),
            filter: (_, _, _, proj) => proj.ParentStatus == "Approved");

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, filters, false, [abstractParent, concreteParent, child]);
    }

    // Projection record types for the tests
    // ReSharper disable NotAccessedPositionalProperty.Local
    record AbstractNavStatusProjection(string? ParentStatus, Guid ChildId);
    record AbstractNavMultiPropertyProjection(string? ParentProperty, string? ParentStatus, Guid ChildId);
}
