using GraphQL.Types;

namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    FieldType AddSingleField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class;

    FieldType AddSingleField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class;
}