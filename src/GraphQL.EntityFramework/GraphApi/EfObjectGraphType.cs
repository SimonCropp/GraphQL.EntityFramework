namespace GraphQL.EntityFramework;

public class EfObjectGraphType<TDbContext, TSource>(IEfGraphQLService<TDbContext> graphQlService) :
    ObjectGraphType<TSource>
    where TDbContext : DbContext
{
    public IEfGraphQLService<TDbContext> GraphQlService { get; } = graphQlService;

    // The Field methods return an EfFieldBuilder so projection-based resolvers
    // can be configured without passing the service
    public override EfFieldBuilder<TDbContext, TSource, TReturnType> Field<TGraphType, TReturnType>(string name) =>
        Wrap(base.Field<TGraphType, TReturnType>(name));

    public override EfFieldBuilder<TDbContext, TSource, object> Field<TGraphType>(string name) =>
        Wrap(base.Field<TGraphType>(name));

    public override EfFieldBuilder<TDbContext, TSource, TReturnType> Field<TReturnType>(string name, bool nullable = false) =>
        Wrap(base.Field<TReturnType>(name, nullable));

    public override EfFieldBuilder<TDbContext, TSource, object> Field(string name, Type type) =>
        Wrap(base.Field(name, type));

    public override EfFieldBuilder<TDbContext, TSource, object> Field(string name, IGraphType type) =>
        Wrap(base.Field(name, type));

    public override EfFieldBuilder<TDbContext, TSource, TProperty> Field<TProperty>(string name, Expression<Func<TSource, TProperty>> expression) =>
        Wrap(base.Field(name, expression));

    public override EfFieldBuilder<TDbContext, TSource, TProperty> Field<TProperty>(string name, Expression<Func<TSource, TProperty>> expression, bool nullable) =>
        Wrap(base.Field(name, expression, nullable));

    public override EfFieldBuilder<TDbContext, TSource, TProperty> Field<TProperty>(string name, Expression<Func<TSource, TProperty>> expression, Type type) =>
        Wrap(base.Field(name, expression, type));

    public override EfFieldBuilder<TDbContext, TSource, TProperty> Field<TProperty>(Expression<Func<TSource, TProperty>> expression) =>
        Wrap(base.Field(expression));

    public override EfFieldBuilder<TDbContext, TSource, TProperty> Field<TProperty>(Expression<Func<TSource, TProperty>> expression, bool nullable) =>
        Wrap(base.Field(expression, nullable));

    public override EfFieldBuilder<TDbContext, TSource, TProperty> Field<TProperty>(Expression<Func<TSource, TProperty>> expression, Type type) =>
        Wrap(base.Field(expression, type));

    EfFieldBuilder<TDbContext, TSource, TReturn> Wrap<TReturn>(FieldBuilder<TSource, TReturn> builder) =>
        builder as EfFieldBuilder<TDbContext, TSource, TReturn> ?? new(builder.FieldType, GraphQlService);

    /// <summary>
    /// Map all un-mapped properties. Underlying behaviour is:
    ///
    ///  * Calls AddNavigationField for all non-list EF navigation properties.
    ///  * Calls AddNavigationListField for all EF navigation properties.
    ///  * Calls <see cref="ComplexGraphType{TSourceType}.AddField"/> for all other properties
    /// </summary>
    /// <param name="exclusions">A list of property names to exclude from mapping.</param>
    public void AutoMap(IReadOnlyList<string>? exclusions = null) =>
        Mapper<TDbContext>.AutoMap(this, GraphQlService, exclusions);

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TReturn, TProjection>(
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddNavigationConnectionField(this, name, projection, resolve, graphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TReturn>(
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddNavigationConnectionField(this, name, projection, graphType);

    public FieldBuilder<TSource, TReturn> AddNavigationField<TReturn, TProjection>(
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, TReturn?> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddNavigationField(this, name, projection, resolve, graphType);

    public FieldBuilder<TSource, TReturn> AddNavigationField<TReturn>(
        string name,
        Expression<Func<TSource, TReturn?>> projection,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddNavigationField(this, name, projection, graphType);

    public FieldBuilder<TSource, TReturn> AddNavigationListField<TReturn, TProjection>(
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddNavigationListField(this, name, projection, resolve, graphType, omitQueryArguments);

    public FieldBuilder<TSource, TReturn> AddNavigationListField<TReturn>(
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddNavigationListField(this, name, projection, graphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddQueryConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TReturn>?> resolve,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, resolve, graphType);

    public FieldBuilder<TSource, TReturn> AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>?> resolve,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType, omitQueryArguments);

    public FieldBuilder<TSource, TReturn> AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>?>?> resolve,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType, omitQueryArguments);

    public TDbContext ResolveDbContext(IResolveFieldContext<TSource> context) =>
        GraphQlService.ResolveDbContext(context);

    public TDbContext ResolveDbContext(IResolveFieldContext context) =>
        GraphQlService.ResolveDbContext(context);

    public FieldBuilder<TSource, TReturn> AddSingleField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<TSource, TReturn> AddSingleField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<TSource, TReturn> AddFirstField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddFirstField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<TSource, TReturn> AddFirstField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class =>
        GraphQlService.AddFirstField(this, name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
}