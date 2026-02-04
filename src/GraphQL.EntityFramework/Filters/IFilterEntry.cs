interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
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

    Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity);
}
