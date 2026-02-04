/// <summary>
/// Tests for ValidateProjectionCompatibility in FilterEntry.
///
/// ValidateProjectionCompatibility only throws for identity projections (_ => _)
/// when the PROJECTION expression itself accesses abstract navigation properties.
/// It does NOT analyze the filter expression.
///
/// For identity projections, the projection body has no member accesses,
/// so the validation always passes regardless of what the filter accesses.
/// </summary>
public partial class IntegrationTests
{
    /// <summary>
    /// Tests that identity projection is allowed when filter accesses abstract navigation.
    /// ValidateProjectionCompatibility only analyzes the projection expression, not the filter.
    /// For identity projection (_ => _), there are no member accesses in the projection to check.
    /// </summary>
    [Fact]
    public void Filter_with_identity_projection_accessing_abstract_navigation_in_filter_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        // DerivedChildEntity.Parent is of abstract type BaseEntity
        // The filter accesses Parent.Status, but the projection is just (_ => _)
        // ValidateProjectionCompatibility only checks the projection, not the filter
        var exception = Record.Exception(() =>
            filters.For<DerivedChildEntity>().Add(
                projection: _ => _,
                filter: (_, _, _, entity) => entity.Parent != null && entity.Parent.Status == "Approved"));

        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that explicit projections accessing abstract navigation properties are allowed.
    /// ValidateProjectionCompatibility only restricts IDENTITY projections, not explicit ones.
    /// </summary>
    [Fact]
    public void Filter_with_explicit_projection_accessing_abstract_navigation_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        // Explicit projection extracts specific properties from abstract navigation
        // This is the RECOMMENDED approach for accessing abstract navigations
        var exception = Record.Exception(() =>
            filters.For<DerivedChildEntity>().Add(
                projection: e => new { e.Id, ParentStatus = e.Parent != null ? e.Parent.Status : null },
                filter: (_, _, _, proj) => proj.ParentStatus == "Approved"));

        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that identity projection is allowed when filter only accesses
    /// concrete (non-navigation) properties.
    /// </summary>
    [Fact]
    public void Filter_with_identity_projection_accessing_concrete_properties_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        var exception = Record.Exception(() =>
            filters.For<DerivedChildEntity>().Add(
                projection: _ => _,
                filter: (_, _, _, entity) => entity.Property == "Test"));

        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that identity projection is allowed when filter accesses
    /// a concrete (non-abstract) navigation property.
    /// </summary>
    [Fact]
    public void Filter_with_identity_projection_accessing_concrete_navigation_does_not_throw()
    {
        var filters = new Filters<IntegrationDbContext>();

        // DerivedChildEntity.TypedParent is of concrete type DerivedWithNavigationEntity
        var exception = Record.Exception(() =>
            filters.For<DerivedChildEntity>().Add(
                projection: _ => _,
                filter: (_, _, _, entity) => entity.TypedParent != null && entity.TypedParent.Property == "Test"));

        Assert.Null(exception);
    }
}
