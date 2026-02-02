public class FilterEntryValidationTests
{
    class TestEntity
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
        public Guid? ParentId { get; set; }
        public AbstractParent? Parent { get; set; }
        public ConcreteParent? ConcreteParent { get; set; }
    }

    abstract class AbstractParent
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
    }

    class ConcreteParent
    {
        public Guid Id { get; set; }
        public string? Property { get; set; }
    }

    class TestDbContext : DbContext;

    [Fact]
    public void Explicit_projection_with_abstract_navigation_property_succeeds()
    {
        var filters = new Filters<TestDbContext>();

        // Explicit projections that extract specific properties through abstract navigations are ALLOWED
        // because EF Core can project scalar properties through abstract navigations efficiently.
        // Only identity projections are problematic.
        filters.For<TestEntity>().Add(
            projection: e => e.Parent!.Property,
            filter: (_, _, _, prop) => prop == "test");

        // Verify the filter was registered with the expected property
        var requiredProps = filters.GetRequiredFilterProperties<TestEntity>();
        Assert.Contains("Parent.Property", requiredProps);
    }

    [Fact]
    public void Concrete_navigation_allows_identity_projection()
    {
        var filters = new Filters<TestDbContext>();

        // Should not throw - concrete navigation
        filters.For<TestEntity>().Add(
            projection: e => e,
            filter: (_, _, _, e) => e.ConcreteParent!.Property == "test");
    }

    [Fact]
    public void Anonymous_type_projection_with_abstract_navigation_succeeds()
    {
        var filters = new Filters<TestDbContext>();

        // Explicit projections (including anonymous types) that extract specific properties
        // through abstract navigations are ALLOWED - EF Core handles this efficiently.
        filters.For<TestEntity>().Add(
            projection: e => new { e.Id, ParentProp = e.Parent!.Property },
            filter: (_, _, _, proj) => proj.ParentProp == "test");

        // Verify the filter was registered with expected properties
        var requiredProps = filters.GetRequiredFilterProperties<TestEntity>();
        Assert.Contains("Id", requiredProps);
        Assert.Contains("Parent.Property", requiredProps);
    }

    [Fact]
    public void Projection_without_abstract_navigation_succeeds()
    {
        var filters = new Filters<TestDbContext>();

        // Should not throw - only accessing scalar properties
        filters.For<TestEntity>().Add(
            projection: e => new { e.Id, e.Property },
            filter: (_, _, _, proj) => proj.Property == "test");
    }

    [Fact]
    public void Identity_projection_without_navigation_succeeds()
    {
        var filters = new Filters<TestDbContext>();

        // Should not throw - identity projection is fine if not accessing navigations
        // (the analyzer will catch if the filter accesses navigations)
        filters.For<TestEntity>().Add(
            projection: e => e,
            filter: (_, _, _, e) => e.Property == "test");
    }

    [Fact]
    public void Identity_projection_with_abstract_navigation_in_filter_not_caught_at_runtime()
    {
        var filters = new Filters<TestDbContext>();

        // Identity projection where filter accesses abstract navigation
        // This is NOT caught at runtime because:
        // 1. The projection `e => e` has no property accesses to analyze
        // 2. The filter is a compiled delegate, not an expression tree
        // The GQLEF007 analyzer catches this at compile time instead.
        filters.For<TestEntity>().Add(
            projection: e => e,
            filter: (_, _, _, e) => e.Parent!.Property == "test");

        // Identity projection extracts no properties
        var requiredProps = filters.GetRequiredFilterProperties<TestEntity>();
        Assert.Empty(requiredProps);
    }
}
