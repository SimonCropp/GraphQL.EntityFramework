namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    #region AddProjectedNavigationConnectionField

    // Simple transform (sync)
    ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class;

    // Simple transform (async)
    ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync)
    ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async)
    ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class;

    #endregion
}
