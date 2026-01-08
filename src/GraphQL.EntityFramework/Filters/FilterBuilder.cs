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
    ///     projection: p => new { p.Quantity, p.Price },
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
    ///     projection: p => new { p.CategoryId },
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
}
