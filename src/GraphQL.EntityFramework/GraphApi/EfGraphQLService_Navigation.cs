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
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?> resolve,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            Guard.AgainstNull(nameof(resolve), resolve);

            var field = BuildNavigationField(name, resolve, includeNames, graphType, description);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField<TSource, TReturn>(name, null, includeNames, graphType, description);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve,
            IEnumerable<string>? includeNames,
            Type? graphType,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

            var fieldType = new FieldType
            {
                Name = name,
                Type = graphType,
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames),
                Description = description
            };

            if (resolve != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, TReturn?>(
                    async context =>
                    {
                        var efFieldContext = BuildContext(context);

                        var result = resolve(efFieldContext);
                        if (await efFieldContext.Filters.ShouldInclude(context.UserContext, result))
                        {
                            return result;
                        }

                        return null;
                    });
            }

            return fieldType;
        }
    }
}