namespace GraphQL.EntityFramework;

public class QueryGraphType<TDbContext>(IEfGraphQLService<TDbContext> graphQlService) :
    ObjectGraphType
    where TDbContext : DbContext
{
    public IEfGraphQLService<TDbContext> GraphQlService { get; } = graphQlService;

    public TDbContext ResolveDbContext<TSource>(IResolveFieldContext<TSource> context) =>
        GraphQlService.ResolveDbContext(context);

    public TDbContext ResolveDbContext(IResolveFieldContext context) =>
        GraphQlService.ResolveDbContext(context);

    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TReturn>?> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, resolve, graphType);

    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>?> resolve,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType, omitQueryArguments);

    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>?>?> resolve,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType, omitQueryArguments);

    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        string name = nameof(TReturn),
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        string name = nameof(TReturn),
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        string name = nameof(TReturn),
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddFirstField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        string name = nameof(TReturn),
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddFirstField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class =>
        GraphQlService.AddIncludes(query, context);

    #region AddProjectedSingleField (resolve returns projected IQueryable)

    /// <summary>
    /// Adds a field that returns a single projected result (throws if multiple match).
    /// Simplified API - resolve function returns IQueryable already projected via Select().
    /// </summary>
    /// <example>
    /// AddProjectedSingleField(
    ///     name: "parentName",
    ///     resolve: ctx => ctx.DbContext.Parents.Where(p => p.Id == id).Select(p => p.Name),
    ///     transform: name => name.ToUpper())
    /// </example>
    public FieldBuilder<object, TReturn> AddProjectedSingleField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, transform, graphType, nullable);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, transform, graphType, nullable);

    #endregion

    #region AddProjectedFirstField (resolve returns projected IQueryable)

    /// <summary>
    /// Adds a field that returns the first projected result (or null if none match).
    /// Simplified API - resolve function returns IQueryable already projected via Select().
    /// </summary>
    public FieldBuilder<object, TReturn> AddProjectedFirstField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, transform, graphType, nullable);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, transform, graphType, nullable);

    #endregion

    #region AddProjectedQueryField (resolve returns projected IQueryable)

    /// <summary>
    /// Adds a field that returns a list of projected results.
    /// Simplified API - resolve function returns IQueryable already projected via Select().
    /// </summary>
    /// <example>
    /// AddProjectedQueryField(
    ///     name: "parentNames",
    ///     resolve: ctx => ctx.DbContext.Parents.Select(p => p.Name),
    ///     transform: name => name.ToUpper())
    /// </example>
    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, transform, itemGraphType, omitQueryArguments);

    #endregion

    #region AddProjectedQueryConnectionField (resolve returns projected IOrderedQueryable)

    /// <summary>
    /// Adds a paginated connection field that returns projected results.
    /// Simplified API - resolve function returns IOrderedQueryable already projected via Select().
    /// </summary>
    /// <example>
    /// AddProjectedQueryConnectionField(
    ///     name: "parentNamesConnection",
    ///     resolve: ctx => ctx.DbContext.Parents.Select(p => p.Name).OrderBy(n => n),
    ///     transform: name => name.ToUpper())
    /// </example>
    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, transform, itemGraphType, omitQueryArguments);

    #endregion
}
