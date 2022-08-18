using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework;

//Navigation fields will always be on a typed graph. so use ComplexGraphType not IComplexGraphType
public partial interface IEfGraphQLService<TDbContext>
{
    ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        IEnumerable<string>? includeNames = null,
        int pageSize = 10)
        where TReturn : class;
}