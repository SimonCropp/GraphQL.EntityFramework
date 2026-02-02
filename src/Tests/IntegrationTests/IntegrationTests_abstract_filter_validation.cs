public partial class IntegrationTests
{
    /// <summary>
    /// Test that explicit projection accessing properties through abstract navigation succeeds.
    /// DerivedChildEntity.Parent is BaseEntity (abstract), but explicit projections that extract
    /// specific scalar properties are ALLOWED because EF Core can project scalar properties
    /// through abstract navigations efficiently. Only identity projections are problematic.
    /// </summary>
    [Fact]
    public void Explicit_projection_with_abstract_navigation_property_succeeds()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Projection accesses Parent.Property where Parent is BaseEntity (abstract)
        // This is ALLOWED - EF Core generates efficient JOIN to get only the needed column
        filters.For<DerivedChildEntity>().Add(
            projection: c => c.Parent!.Property,
            filter: (_, _, _, prop) => prop == "test");

        // Verify the filter was registered with the expected property
        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Contains("Parent.Property", requiredProps);
    }

    /// <summary>
    /// Test that anonymous type projection with abstract navigation succeeds.
    /// Explicit projections (including anonymous types) that extract specific properties
    /// through abstract navigations are ALLOWED - EF Core handles this efficiently.
    /// </summary>
    [Fact]
    public void Anonymous_type_projection_with_abstract_navigation_succeeds()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Anonymous type projection extracting properties through abstract Parent
        // This is ALLOWED - EF Core generates efficient SQL with JOINs
        filters.For<DerivedChildEntity>().Add(
            projection: c => new { c.Id, ParentProperty = c.Parent!.Property },
            filter: (_, _, _, proj) => proj.ParentProperty == "test");

        // Verify the filter was registered with expected properties
        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Contains("Id", requiredProps);
        Assert.Contains("Parent.Property", requiredProps);
    }

    /// <summary>
    /// Test that explicit projection with concrete (non-abstract) navigation succeeds.
    /// DerivedChildEntity.TypedParent is DerivedWithNavigationEntity (concrete),
    /// so projection through it should be allowed.
    /// </summary>
    [Fact]
    public void Explicit_projection_with_concrete_navigation_succeeds()
    {
        var filters = new Filters<IntegrationDbContext>();

        // DerivedWithNavigationEntity is concrete - should work fine
        filters.For<DerivedChildEntity>().Add(
            projection: c => c.TypedParent!.Property,
            filter: (_, _, _, prop) => prop == "test");

        // Verify the filter was added successfully
        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Contains("TypedParent.Property", requiredProps);
    }

    /// <summary>
    /// Test that identity projection without navigation access succeeds.
    /// Identity projections are fine when the filter only accesses scalar properties.
    /// Runtime validation doesn't analyze the filter, so this succeeds.
    /// </summary>
    [Fact]
    public void Identity_projection_without_navigation_succeeds()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Identity projection with filter that only accesses scalars
        // (Runtime doesn't validate filter, so this is allowed)
        filters.For<DerivedChildEntity>().Add(
            projection: c => c,
            filter: (_, _, _, c) => c is { Property: "test", ParentId: not null });

        // For identity projection, RequiredPropertyNames is empty
        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Empty(requiredProps);
    }

    /// <summary>
    /// Test that the 4-parameter filter syntax (which uses identity projection internally)
    /// does NOT throw at runtime. The analyzer will catch filter accesses at compile time.
    /// Runtime validation only catches explicit projections through abstract navigations.
    /// </summary>
    [Fact]
    public void Four_parameter_filter_with_abstract_navigation_allowed_at_runtime()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Runtime doesn't analyze the filter body, so this is allowed
        // (The analyzer will catch this at compile time)
        filters.For<DerivedChildEntity>().Add(
            filter: (_, _, _, c) => c.ParentId != null);

        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Empty(requiredProps); // Identity projection extracts no properties
    }

    /// <summary>
    /// Test projecting from nested abstract navigation property succeeds.
    /// Explicit projections that extract specific scalar properties are allowed.
    /// </summary>
    [Fact]
    public void Projection_with_nested_abstract_navigation_property_succeeds()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Accessing nested property through abstract parent is allowed for explicit projections
        filters.For<DerivedChildEntity>().Add(
            projection: c => c.Parent!.Status,
            filter: (_, _, _, status) => status == "Active");

        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Contains("Parent.Status", requiredProps);
    }

    /// <summary>
    /// Test that identity projection with filter accessing abstract navigation is NOT caught at runtime.
    /// The runtime cannot analyze the filter delegate. The GQLEF007 analyzer catches this at compile time.
    /// </summary>
    [Fact]
    public void Identity_projection_with_abstract_navigation_in_filter_not_caught_at_runtime()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Identity projection where filter accesses abstract Parent
        // This is NOT caught at runtime because:
        // 1. The projection `c => c` has no property accesses to analyze
        // 2. The filter is a compiled delegate, not an expression tree
        // The GQLEF007 analyzer catches this at compile time instead.
        filters.For<DerivedChildEntity>().Add(
            projection: c => c,
            filter: (_, _, _, c) => c.Parent!.Property == "test");

        // Identity projection extracts no properties
        var requiredProps = filters.GetRequiredFilterProperties<DerivedChildEntity>();
        Assert.Empty(requiredProps);
    }
}
