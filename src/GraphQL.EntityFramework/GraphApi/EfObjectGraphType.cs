namespace GraphQL.EntityFramework;

public class EfObjectGraphType<TDbContext, TSource>(IEfGraphQLService<TDbContext> graphQlService) :
    ObjectGraphType<TSource>
    where TDbContext : DbContext
{
    public IEfGraphQLService<TDbContext> GraphQlService { get; } = graphQlService;

    /// <summary>
    /// Map all un-mapped properties. Underlying behaviour is:
    ///
    ///  * Calls <see cref="IEfGraphQLService{TDbContext}.AddNavigationField{TSource,TReturn}"/> for all non-list EF navigation properties.
    ///  * Calls <see cref="IEfGraphQLService{TDbContext}.AddNavigationListField{TSource,TReturn}"/> for all EF navigation properties.
    ///  * Calls <see cref="ComplexGraphType{TSourceType}.AddField"/> for all other properties
    /// </summary>
    /// <param name="exclusions">A list of property names to exclude from mapping.</param>
    public void AutoMap(IReadOnlyList<string>? exclusions = null) =>
        Mapper<TDbContext>.AutoMap(this, GraphQlService, exclusions);

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class =>
        GraphQlService.AddNavigationConnectionField(this, name, resolve, graphType, includeNames);

    public FieldBuilder<TSource, TReturn> AddNavigationField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class =>
        GraphQlService.AddNavigationField(this, name, resolve, graphType, includeNames);

    public FieldBuilder<TSource, TReturn> AddNavigationListField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class =>
        GraphQlService.AddNavigationListField(this, name, resolve, graphType, includeNames);

    public ConnectionBuilder<TSource> AddQueryConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, resolve, graphType);

    public FieldBuilder<TSource, TReturn> AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType);

    public TDbContext ResolveDbContext(IResolveFieldContext<TSource> context) =>
        GraphQlService.ResolveDbContext(context);

    public TDbContext ResolveDbContext(IResolveFieldContext context) =>
        GraphQlService.ResolveDbContext(context);

    public FieldBuilder<TSource, TReturn> AddSingleField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments);
}