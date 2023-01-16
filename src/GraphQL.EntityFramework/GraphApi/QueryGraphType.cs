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

    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, resolve, graphType);

    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType);

    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        string name = nameof(TReturn),
        bool nullable = false,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments);

    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class =>
        GraphQlService.AddIncludes(query, context);
}