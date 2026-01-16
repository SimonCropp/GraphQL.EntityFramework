namespace GraphQL.EntityFramework;

//Navigation fields will always be on a typed graph. so use ComplexGraphType not IComplexGraphType
public partial interface IEfGraphQLService<TDbContext>
{
    FieldBuilder<TSource, TReturn> AddNavigationField<TSource, TReturn>(ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddNavigationField<TSource, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, TReturn?> resolve,
        Type? graphType = null)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddNavigationField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TReturn?>> projection,
        Type? graphType = null)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class;
}