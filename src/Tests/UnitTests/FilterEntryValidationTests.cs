using GraphQL.EntityFramework;

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

    class TestDbContext : DbContext
    {
    }

    [Fact]
    public void Explicit_projection_with_abstract_navigation_property_throws()
    {
        var filters = new Filters<TestDbContext>();

        // This should throw - projection directly accesses abstract navigation property
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            filters.For<TestEntity>().Add(
                projection: e => e.Parent!.Property,
                filter: (_, _, _, prop) => prop == "test");
        });

        Assert.Contains("abstract navigation", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parent", exception.Message);
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
    public void Anonymous_type_projection_with_abstract_navigation_throws()
    {
        var filters = new Filters<TestDbContext>();

        // This should also throw - even with anonymous type, we're projecting from abstract navigation
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            filters.For<TestEntity>().Add(
                projection: e => new { e.Id, ParentProp = e.Parent!.Property },
                filter: (_, _, _, proj) => proj.ParentProp == "test");
        });

        Assert.Contains("abstract navigation", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parent", exception.Message);
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
}
