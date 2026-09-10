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

    public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> Navigations { get; }
}