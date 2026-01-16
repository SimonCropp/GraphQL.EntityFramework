namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    #region AddProjectedQueryConnectionField - TSource generic

    // Simple transform (sync)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Simple transform (async)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (sync)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (async)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (sync)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (async)
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    #endregion

    #region AddProjectedQueryConnectionField - object source

    // Simple transform (sync)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Simple transform (async)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (sync)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (async)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (sync)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (async)
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    #endregion
}
