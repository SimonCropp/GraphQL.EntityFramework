namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    #region AddProjectedField (simplified navigation projection)

    /// <summary>
    /// Adds a field that projects from the source entity and transforms the result.
    /// </summary>
    /// <example>
    /// AddProjectedField(
    ///     name: "childName",
    ///     projection: parent => parent.Child.Name,
    ///     transform: name => name.ToUpper())
    /// </example>
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class;

    /// <summary>
    /// Adds a field that projects from the source entity and transforms the result asynchronously.
    /// </summary>
    FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class;

    #endregion

    #region AddProjectedListField (simplified list projection)

    /// <summary>
    /// Adds a field that projects a list from the source entity and transforms each item.
    /// </summary>
    /// <example>
    /// AddProjectedListField(
    ///     name: "childNames",
    ///     projection: parent => parent.Children.Select(c => c.Name),
    ///     transform: name => name.ToUpper())
    /// </example>
    FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedListField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class;

    /// <summary>
    /// Adds a field that projects a list from the source entity and transforms each item asynchronously.
    /// </summary>
    FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedListField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class;

    #endregion

    #region AddProjectedConnectionField (simplified connection projection)

    /// <summary>
    /// Adds a paginated connection field that projects from the source entity and transforms each item.
    /// </summary>
    /// <example>
    /// AddProjectedConnectionField(
    ///     name: "childNamesConnection",
    ///     projection: parent => parent.Children.Select(c => c.Name),
    ///     transform: name => name.ToUpper())
    /// </example>
    ConnectionBuilder<TSource> AddProjectedConnectionField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class;

    /// <summary>
    /// Adds a paginated connection field that projects from the source entity and transforms each item asynchronously.
    /// </summary>
    ConnectionBuilder<TSource> AddProjectedConnectionField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class;

    #endregion

    #region AddProjectedSingleField (simplified - resolve returns projected IQueryable)

    /// <summary>
    /// Adds a field that returns a single projected result (throws if multiple match).
    /// The resolve function should return an IQueryable already projected via Select().
    /// </summary>
    /// <example>
    /// AddProjectedSingleField(
    ///     name: "parentName",
    ///     resolve: ctx => ctx.DbContext.Parents.Where(p => p.Id == id).Select(p => p.Name),
    ///     transform: name => name.ToUpper())
    /// </example>
    FieldBuilder<TSource, TReturn> AddProjectedSingleField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns a single projected result with async transform.
    /// </summary>
    FieldBuilder<TSource, TReturn> AddProjectedSingleField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns a single projected result (object source variant).
    /// </summary>
    FieldBuilder<object, TReturn> AddProjectedSingleField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns a single projected result with async transform (object source variant).
    /// </summary>
    FieldBuilder<object, TReturn> AddProjectedSingleField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    #endregion

    #region AddProjectedFirstField (simplified - resolve returns projected IQueryable)

    /// <summary>
    /// Adds a field that returns the first projected result (or null if none match).
    /// The resolve function should return an IQueryable already projected via Select().
    /// </summary>
    FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns the first projected result with async transform.
    /// </summary>
    FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns the first projected result (object source variant).
    /// </summary>
    FieldBuilder<object, TReturn> AddProjectedFirstField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns the first projected result with async transform (object source variant).
    /// </summary>
    FieldBuilder<object, TReturn> AddProjectedFirstField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class;

    #endregion

    #region AddProjectedQueryField (simplified - resolve returns projected IQueryable)

    /// <summary>
    /// Adds a field that returns a list of projected results.
    /// The resolve function should return an IQueryable already projected via Select().
    /// </summary>
    /// <example>
    /// AddProjectedQueryField(
    ///     name: "parentNames",
    ///     resolve: ctx => ctx.DbContext.Parents.Select(p => p.Name),
    ///     transform: name => name.ToUpper())
    /// </example>
    FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedQueryField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns a list of projected results with async transform.
    /// </summary>
    FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedQueryField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns a list of projected results (object source variant).
    /// </summary>
    FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    /// <summary>
    /// Adds a field that returns a list of projected results with async transform (object source variant).
    /// </summary>
    FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    #endregion

    #region AddProjectedQueryConnectionField (simplified - resolve returns projected IOrderedQueryable)

    /// <summary>
    /// Adds a paginated connection field that returns projected results.
    /// The resolve function should return an IOrderedQueryable already projected via Select().
    /// </summary>
    /// <example>
    /// AddProjectedQueryConnectionField(
    ///     name: "parentNamesConnection",
    ///     resolve: ctx => ctx.DbContext.Parents.Select(p => p.Name).OrderBy(n => n),
    ///     transform: name => name.ToUpper())
    /// </example>
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    /// <summary>
    /// Adds a paginated connection field that returns projected results with async transform.
    /// </summary>
    ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    /// <summary>
    /// Adds a paginated connection field (object source variant).
    /// </summary>
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    /// <summary>
    /// Adds a paginated connection field with async transform (object source variant).
    /// </summary>
    ConnectionBuilder<object> AddProjectedQueryConnectionField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    #endregion
}
