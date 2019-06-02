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
        public FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn> resolve,
            Type graphType = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(name, resolve, includeNames, graphType);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn> resolve,
            IEnumerable<string> includeNames,
            Type graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            graphType = GraphTypeFinder.FindGraphType<TReturn>(graphType);
            return new FieldType
            {
                Name = name,
                Type = graphType,
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames),
                Resolver = new AsyncFieldResolver<TSource, TReturn>(async context =>
                {
                    var result = resolve(BuildEfContextFromGraphQlContext(context));
                    if (await filters.ShouldInclude(context.UserContext, result))
                    {
                        return result;
                    }

                    return null;
                })
            };
        }
    }
}