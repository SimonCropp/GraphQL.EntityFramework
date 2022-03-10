using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

public class EfObjectGraphType<TDbContext, TSource> :
    ObjectGraphType<TSource>
    where TDbContext : DbContext
{
    public IEfGraphQLService<TDbContext> GraphQlService { get; }

    public EfObjectGraphType(IEfGraphQLService<TDbContext> graphQlService) => GraphQlService = graphQlService;

    /// <summary>
    /// Map all un-mapped properties. Underlying behaviour is:
    ///
    ///  * Calls <see cref="IEfGraphQLService{TDbContext}.AddNavigationField{TSource,TReturn}"/> for all non-list EF navigation properties.
    ///  * Calls <see cref="IEfGraphQLService{TDbContext}.AddNavigationListField{TSource,TReturn}"/> for all EF navigation properties.
    ///  * Calls <see cref="ComplexGraphType{TSourceType}.AddField"/> for all other properties
    /// </summary>
    /// <param name="exclusions">A list of property names to exclude from mapping.</param>
    public void AutoMap(IReadOnlyList<string>? exclusions = null) =>
        Mapper<TDbContext>.AutoMap(this, GraphQlService, exclusions);

    public void AddNavigationConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        IEnumerable<string>? includeNames = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddNavigationConnectionField(this, name, resolve, graphType, arguments, includeNames, pageSize, description);

    public FieldType AddNavigationField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddNavigationField(this, name, resolve, graphType, includeNames, description);

    public FieldType AddNavigationListField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        IEnumerable<string>? includeNames = null,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddNavigationListField(this, name, resolve, graphType, arguments, includeNames, description);

    public void AddQueryConnectionField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize, description);

    public FieldType AddQueryField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddQueryField(this, name, resolve, graphType, arguments, description);

    public TDbContext ResolveDbContext(IResolveFieldContext<TSource> context) =>
        GraphQlService.ResolveDbContext(context);

    public TDbContext ResolveDbContext(IResolveFieldContext context) =>
        GraphQlService.ResolveDbContext(context);

    public FieldType AddSingleField<TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class =>
        GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, arguments, nullable, description);
}