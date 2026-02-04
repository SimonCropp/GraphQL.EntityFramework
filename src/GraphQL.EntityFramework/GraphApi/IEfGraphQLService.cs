namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    TDbContext ResolveDbContext(IResolveFieldContext context);

    public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> Navigations { get; }
}