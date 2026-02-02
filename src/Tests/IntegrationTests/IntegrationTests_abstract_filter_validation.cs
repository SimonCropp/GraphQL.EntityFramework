public partial class IntegrationTests
{
    /// <summary>
    /// Test that explicit projection accessing properties through abstract navigation throws.
    /// DerivedChildEntity.Parent is BaseEntity (abstract), so projecting Parent.Property
    /// should throw because abstract types cannot be projected by EF Core.
    /// </summary>
    [Fact]
    public void Explicit_projection_with_abstract_navigation_property_throws()
    {
        var filters = new Filters<IntegrationDbContext>();

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // Projection accesses Parent.Property where Parent is BaseEntity (abstract)
            filters.For<DerivedChildEntity>().Add(
                projection: c => c.Parent!.Property,
                filter: (_, _, _, prop) => prop == "test");
        });

        Assert.Contains("abstract navigation", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parent", exception.Message);
    }

    /// <summary>
    /// Test that anonymous type projection with abstract navigation also throws.
    /// Even with anonymous types, we cannot project from abstract navigations.
    /// </summary>
    [Fact]
    public void Anonymous_type_projection_with_abstract_navigation_throws()
    {
        var filters = new Filters<IntegrationDbContext>();

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // Even with anonymous type, projecting from abstract Parent throws
            filters.For<DerivedChildEntity>().Add(
                projection: c => new { c.Id, ParentProperty = c.Parent!.Property },
                filter: (_, _, _, proj) => proj.ParentProperty == "test");
        });

        Assert.Contains("abstract navigation", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parent", exception.Message);
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
            filter: (_, _, _, c) => c.Property == "test" && c.ParentId != null);

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
    /// Test projecting from nested abstract navigation property.
    /// </summary>
    [Fact]
    public void Projection_with_nested_abstract_navigation_property_throws()
    {
        var filters = new Filters<IntegrationDbContext>();

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // Accessing nested property through abstract parent
            filters.For<DerivedChildEntity>().Add(
                projection: c => c.Parent!.Status,
                filter: (_, _, _, status) => status == "Active");
        });

        Assert.Contains("abstract navigation", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Parent", exception.Message);
    }
}
