namespace GraphQL.EntityFramework;

/// <summary>
/// The part of a service that does not depend on the context type. The generated where and
/// orderBy input types read the model through it.
/// </summary>
public interface IEfGraphQLService
{
    IModel Model { get; }
}

public partial interface IEfGraphQLService<TDbContext> :
    IEfGraphQLService
    where TDbContext : DbContext
{
    TDbContext ResolveDbContext(IResolveFieldContext context);

    Filters<TDbContext>? ResolveFilters(IResolveFieldContext context);

    /// <summary>
    /// Narrows <paramref name="query"/> to what the request selected under the field being
    /// resolved, the way the query fields do: a select projection where the entity can be
    /// projected, otherwise includes. For a resolver that builds a query of its own, such as a
    /// mutation reading back the entity it returns.
    /// </summary>
    /// <param name="context">The context of the field being resolved. The field has to return the graph type of <typeparamref name="TItem"/>.</param>
    /// <param name="query">The query to narrow. Call AsSplitQuery on it if the request might select a collection; it is removed when none is.</param>
    IQueryable<TItem> ApplyProjection<TItem>(IResolveFieldContext context, IQueryable<TItem> query)
        where TItem : class;

    public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> Navigations { get; }
}