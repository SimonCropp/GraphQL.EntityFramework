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

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TReturn>(
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddNavigationConnectionField(this, name, projection, graphType);

    public FieldBuilder<TSource, TReturn> AddNavigationField<TReturn>(
        string name,
        Expression<Func<TSource, TReturn?>> projection,
        Type? graphType = null)
        where TReturn : class =>
        GraphQlService.AddNavigationField(this, name, projection, graphType);

    public FieldBuilder<TSource, TReturn> AddNavigationListField<TReturn>(
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        GraphQlService.AddNavigationListField(this, name, projection, graphType, omitQueryArguments);

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