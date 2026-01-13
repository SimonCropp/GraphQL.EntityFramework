namespace GraphQL.EntityFramework;

public class EfInterfaceGraphType<TDbContext, TSource>(
    IEfGraphQLService<TDbContext> graphQlService,
    params Expression<Func<TSource, object?>>[]? excludedProperties) :
        AutoRegisteringInterfaceGraphType<TSource>(excludedProperties)
    where TDbContext : DbContext
{
    public EfInterfaceGraphType(IEfGraphQLService<TDbContext> graphQlService):this(graphQlService, null)
    {
    }

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

    #region AddProjectedNavigationField

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, graphType, includeNames);

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, graphType, includeNames);

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, graphType, includeNames);

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, graphType, includeNames);

    #endregion

    #region AddProjectedNavigationListField

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, itemGraphType, includeNames, omitQueryArguments);

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, itemGraphType, includeNames, omitQueryArguments);

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, itemGraphType, includeNames, omitQueryArguments);

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(this, name, null, projection, transform, itemGraphType, includeNames, omitQueryArguments);

    #endregion
}