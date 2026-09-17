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

/// <summary>
/// A filter that decides for many items in one call. The items are projected first, and the
/// filter is passed the distinct projections.
/// </summary>
interface IBatchFilterEntry<TDbContext> :
    IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    object? Project(object entity);

    /// <summary>
    /// Returns whether each of <paramref name="projections"/> is included.
    /// </summary>
    Task<Func<object?, bool>> Filter(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        IReadOnlyCollection<object?> projections);
}
