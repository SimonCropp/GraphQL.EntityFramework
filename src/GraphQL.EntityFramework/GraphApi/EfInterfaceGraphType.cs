namespace GraphQL.EntityFramework;

public class EfInterfaceGraphType<TDbContext, TSource>(IEfGraphQLService<TDbContext> graphQlService) :
    InterfaceGraphType<TSource>
    where TDbContext : DbContext
{
    public IEfGraphQLService<TDbContext> GraphQlService { get; } = graphQlService;

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TReturn>(
        string name,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddNavigationConnectionField<TSource, TReturn>(this, name, null, graphType, includeNames, omitQueryArguments);

    public FieldBuilder<TSource, TReturn> AddNavigationField<TReturn>(
        string name,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class =>
        GraphQlService.AddNavigationField<TSource, TReturn>(this, name, null, graphType, includeNames);

    public FieldBuilder<TSource, TReturn> AddNavigationListField<TReturn>(
        string name,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddNavigationListField<TSource, TReturn>(this, name, null, graphType, includeNames, omitQueryArguments);

    public ConnectionBuilder<TSource> AddQueryConnectionField<TReturn>(
        string name,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, (Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TReturn>?>?>?)null, graphType);

    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        string name,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, (Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>?>?>?)null, graphType, omitQueryArguments);

    public TDbContext ResolveDbContext(IResolveFieldContext<TSource> context) =>
        GraphQlService.ResolveDbContext(context);

    public TDbContext ResolveDbContext(IResolveFieldContext context) =>
        GraphQlService.ResolveDbContext(context);
}