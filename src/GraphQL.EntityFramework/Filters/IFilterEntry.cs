interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// The entity type this filter applies to.
    /// </summary>
    Type EntityType { get; }

    /// <summary>
    /// The raw property names extracted from the filter's projection expression.
    /// Used for testing and diagnostics.
    /// </summary>
    IReadOnlySet<string> RequiredPropertyNames { get; }

    /// <summary>
    /// Add this filter's requirements to the projection.
    /// Returns the updated projection with filter-required fields and navigations merged in.
    /// </summary>
    /// <param name="projection">The current projection to merge requirements into.</param>
    /// <param name="navigationProperties">Navigation property metadata for the entity type.</param>
    /// <returns>Updated projection with filter requirements included.</returns>
    FieldProjectionInfo AddRequirements(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties);

    /// <summary>
    /// Get navigation names where the navigation type is abstract.
    /// These navigations need Include() instead of projection because
    /// abstract types cannot be instantiated in a Select expression.
    /// </summary>
    /// <param name="navigationProperties">Navigation property metadata for the entity type.</param>
    /// <returns>Navigation names that require Include due to abstract types.</returns>
    IEnumerable<string> GetAbstractNavigationIncludes(
        IReadOnlyDictionary<string, Navigation>? navigationProperties);

    Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity);
}
