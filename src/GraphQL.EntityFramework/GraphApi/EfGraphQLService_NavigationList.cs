using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
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
            Guard.AgainstNull(nameof(graph), graph);
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var hasId = keyNames.ContainsKey(typeof(TReturn));
            FieldType field = new()
            {
                Name = name,
                Description = description,
                Type = MakeListGraphType<TReturn>(itemGraphType),
                Arguments = ArgumentAppender.GetQueryArguments(arguments,hasId),
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames)
            };

            if (resolve != null)
            {
                field.Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    context =>
                    {
                        var fieldContext = BuildContext(context);
                        var result = resolve(fieldContext);
                        result = result.ApplyGraphQlArguments(hasId, context);
                        return fieldContext.Filters.ApplyFilter(result, context.UserContext);
                    });
            }

            return graph.AddField(field);
        }
    }
}