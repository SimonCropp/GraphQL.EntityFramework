using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldType AddNavigationListField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        IEnumerable<string>? includeNames = null,
        string? description = null)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var field = new FieldType
        {
            Name = name,
            Description = description,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(arguments, hasId, true),
        };

        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        if (resolve is not null)
        {
            field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
                async context =>
                {
                    var fieldContext = BuildContext(context);
                    var result = resolve(fieldContext);
                    result = result.ApplyGraphQlArguments(hasId, context);
                    return await fieldContext.Filters.ApplyFilter(result, context.UserContext);
                });
        }

        return graph.AddField(field);
    }
}