namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    #region AddProjectedNavigationField

    // Simple transform (sync)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class;

    // Simple transform (async)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class;

    #endregion

    #region AddProjectedNavigationListField

    // Simple transform (sync)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Simple transform (async)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async)
    FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class;

    #endregion

    #region AddProjectedField

    // Simple transform (sync) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Simple transform (async) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (sync) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (async) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (sync) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (async) - TSource generic
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Simple transform (sync) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Simple transform (async) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (sync) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Context-aware transform (async) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (sync) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + simple transform (async) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (sync) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    // Async resolve + context-aware transform (async) - object source
    FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class;

    #endregion
}
