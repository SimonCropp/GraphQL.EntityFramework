using GraphQL.Types;

namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    void AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class;

    void AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class;
}