using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

public class QueryGraphType<TDbContext> :
    ObjectGraphType
    where TDbContext : DbContext
{
    public QueryGraphType(IEfGraphQLService<TDbContext> graphQlService) =>
        GraphQlService = graphQlService;

    public IEfGraphQLService<TDbContext> GraphQlService { get; }

    public TDbContext ResolveDbContext<TSource>(IResolveFieldContext<TSource> context) =>
        GraphQlService.ResolveDbContext(context);

    public TDbContext ResolveDbContext(IResolveFieldContext context) =>
        GraphQlService.ResolveDbContext(context);

    public void AddQueryConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize, description);

    public FieldType AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType, arguments, description);

    public FieldType AddSingleField<TReturn>(
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        string name = nameof(TReturn),
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, arguments, nullable, description);

    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class =>
        GraphQlService.AddIncludes(query, context);
}