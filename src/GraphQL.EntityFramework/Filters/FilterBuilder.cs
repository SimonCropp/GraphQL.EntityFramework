namespace GraphQL.EntityFramework;

/// <summary>
/// Fluent builder for adding filters with inferred projection types.
/// Allows using anonymous types without specifying TProjection explicitly.
/// </summary>
/// <typeparam name="TDbContext">The DbContext type.</typeparam>
/// <typeparam name="TEntity">The entity type to filter.</typeparam>
public class FilterBuilder<TDbContext, TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    readonly Filters<TDbContext> filters;

    internal FilterBuilder(Filters<TDbContext> filters) =>
        this.filters = filters;

    /// <summary>
    /// Add a synchronous filter with an inferred projection type.
    /// </summary>
    /// <typeparam name="TProjection">The projection type (inferred from the projection expression).</typeparam>
    /// <param name="projection">Expression projecting the entity to the desired type.</param>
    /// <param name="filter">Synchronous filter function to determine if the projected value should be included.</param>
    /// <remarks>
    /// This overload allows using anonymous types for projections:
    /// <code>
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => new { _.Quantity, _.Price },
    ///     filter: (_, _, _, x) => x.Quantity > 0 &amp;&amp; x.Price >= 10);
    /// </code>
    /// </remarks>
    public void Add<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Filters<TDbContext>.Filter<TProjection> filter) =>
        filters.Add(projection, filter);

    /// <summary>
    /// Add an asynchronous filter with an inferred projection type.
    /// </summary>
    /// <typeparam name="TProjection">The projection type (inferred from the projection expression).</typeparam>
    /// <param name="projection">Expression projecting the entity to the desired type.</param>
    /// <param name="filter">Asynchronous filter function to determine if the projected value should be included.</param>
    /// <remarks>
    /// This overload allows using anonymous types for projections:
    /// <code>
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => new { _.CategoryId },
    ///     filter: async (_, dbContext, _, x) =>
    ///     {
    ///         var category = await dbContext.Categories.FindAsync(x.CategoryId);
    ///         return category?.IsVisible == true;
    ///     });
    /// </code>
    /// </remarks>
    public void Add<TProjection>(
        Expression<Func<TEntity, TProjection>> projection,
        Filters<TDbContext>.AsyncFilter<TProjection> filter) =>
        filters.Add(projection, filter);

    /// <summary>
    /// Add a filter using a boolean expression.
    /// </summary>
    /// <param name="filter">Expression that projects to a boolean value and determines if the entity should be included.</param>
    /// <remarks>
    /// This is a simplified overload for boolean properties. The following are equivalent:
    /// <code>
    /// // Simplified syntax
    /// filters.For&lt;Product&gt;().Add(filter: _ => _.IsActive);
    ///
    /// // Explicit syntax
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => _.IsActive,
    ///     filter: (_, _, _, isActive) => isActive);
    /// </code>
    /// </remarks>
    public void Add(Expression<Func<TEntity, bool>> filter) =>
        filters.Add(filter, (_, _, _, value) => value);

    /// <summary>
    /// Add a synchronous filter that doesn't require entity data (e.g., filters based only on user context).
    /// </summary>
    /// <param name="filter">Synchronous filter function that only uses context/user, not entity data.</param>
    /// <remarks>
    /// This overload is useful for filters that only check user permissions or context:
    /// <code>
    /// // Simplified syntax (no projection needed)
    /// filters.For&lt;Product&gt;().Add(
    ///     filter: (_, _, user) => user!.HasPermission("ViewProducts"));
    ///
    /// // Equivalent to:
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => true,  // Projection not used
    ///     filter: (_, _, user, _) => user!.HasPermission("ViewProducts"));
    /// </code>
    /// </remarks>
    public void Add(Func<object, TDbContext, ClaimsPrincipal?, bool> filter) =>
        filters.Add<TEntity, bool>(
            null,
            (userContext, dbContext, userPrincipal, _) => filter(userContext, dbContext, userPrincipal));

    /// <summary>
    /// Add an asynchronous filter that doesn't require entity data (e.g., filters based only on user context).
    /// </summary>
    /// <param name="filter">Asynchronous filter function that only uses context/user, not entity data.</param>
    /// <remarks>
    /// This overload is useful for filters that need async operations without entity data:
    /// <code>
    /// // Simplified syntax (no projection needed)
    /// filters.For&lt;Product&gt;().Add(
    ///     filter: async (_, dbContext, user) =>
    ///     {
    ///         var permissions = await dbContext.GetUserPermissionsAsync(user);
    ///         return permissions.CanView("Products");
    ///     });
    ///
    /// // Equivalent to:
    /// filters.For&lt;Product&gt;().Add(
    ///     projection: _ => true,  // Projection not used
    ///     filter: async (_, dbContext, user, _) =>
    ///     {
    ///         var permissions = await dbContext.GetUserPermissionsAsync(user);
    ///         return permissions.CanView("Products");
    ///     });
    /// </code>
    /// </remarks>
    public void Add(Func<object, TDbContext, ClaimsPrincipal?, Task<bool>> filter) =>
        filters.Add<TEntity, bool>(
            null,
            (userContext, dbContext, userPrincipal, _) => filter(userContext, dbContext, userPrincipal));

    /// <summary>
    /// Add a synchronous filter that operates on the entity itself (identity projection).
    /// Only primary key and foreign key properties should be accessed in the filter.
    /// </summary>
    /// <param name="filter">Synchronous filter function that receives the full entity.</param>
    /// <remarks>
    /// This simplified API is equivalent to:
    /// <code>
    /// filters.For&lt;TEntity&gt;().Add(
    ///     projection: _ => _,
    ///     filter: (userContext, data, userPrincipal, entity) => /* logic */);
    /// </code>
    /// IMPORTANT: Only access primary key (Id, EntityId) or foreign key (ParentId, etc.)
    /// properties. Accessing scalar or navigation properties will cause runtime errors
    /// because they won't be loaded by EF projections.
    /// </remarks>
    public void Add(Filters<TDbContext>.Filter<TEntity> filter) =>
        filters.Add<TEntity, TEntity>(
            projection: _ => _,
            filter: filter);

    /// <summary>
    /// Add an asynchronous filter that operates on the entity itself (identity projection).
    /// Only primary key and foreign key properties should be accessed in the filter.
    /// </summary>
    /// <param name="filter">Asynchronous filter function that receives the full entity.</param>
    /// <remarks>
    /// This simplified API is equivalent to:
    /// <code>
    /// filters.For&lt;TEntity&gt;().Add(
    ///     projection: _ => _,
    ///     filter: async (userContext, data, userPrincipal, entity) => /* logic */);
    /// </code>
    /// IMPORTANT: Only access primary key (Id, EntityId) or foreign key (ParentId, etc.)
    /// properties. Accessing scalar or navigation properties will cause runtime errors.
    /// </remarks>
    public void Add(Filters<TDbContext>.AsyncFilter<TEntity> filter) =>
        filters.Add<TEntity, TEntity>(
            projection: _ => _,
            filter: filter);
}
