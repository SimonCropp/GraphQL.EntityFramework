using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public FieldType AddNavigationListField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(graphType, name, resolve, includeNames, arguments);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string> includeNames,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            graphType = GraphTypeFinder.FindGraphType<TReturn>(graphType);
            var listGraphType = MakeListGraphType(graphType);
            return BuildNavigationField(name, resolve, includeNames, listGraphType, arguments);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string> includeNames,
            Type listGraphType,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames),
                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var result = resolve(context);
                        result = result.ApplyGraphQlArguments(context);
                        return await filters.ApplyFilter(result, context.UserContext);
                    })
            };
        }
    }
}