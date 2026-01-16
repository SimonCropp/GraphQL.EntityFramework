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

    #region AddProjectedField

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedField(this, name, resolve, projection, transform, graphType);

    #endregion

    #region AddProjectedSingleField

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedSingleField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    #endregion

    #region AddProjectedFirstField

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedFirstField(this, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    #endregion

    #region AddProjectedQueryConnectionField

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryConnectionField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    #endregion

    #region AddProjectedQueryField

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        GraphQlService.AddProjectedQueryField(this, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    #endregion
}
