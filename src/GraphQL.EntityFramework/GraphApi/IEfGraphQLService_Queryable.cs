using GraphQL.Types;

namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    FieldType AddQueryField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        string? description = null)
        where TReturn : class;

    FieldType AddQueryField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        string? description = null)
        where TReturn : class;
}