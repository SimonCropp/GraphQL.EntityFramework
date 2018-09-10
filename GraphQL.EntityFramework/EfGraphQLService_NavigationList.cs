using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public FieldType AddNavigationField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField<object, TGraph, TReturn>(name, resolve, includeNames, arguments);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(graphType, name, resolve, includeNames, arguments);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
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
            Guard.AgainstNull(nameof(graphType), graphType);
            var listGraphType = MakeListGraphType(graphType);
            return BuildNavigationField(name, resolve, includeNames, listGraphType, arguments);
        }

        public FieldType AddNavigationField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField<TSource, TGraph, TReturn>(name, resolve, includeNames, arguments);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string> includeNames,
            IEnumerable<QueryArgument> arguments)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var listGraphType = typeof(ListGraphType<TGraph>);
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
                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(async context =>
                    {
                        var result = resolve(context);
                        result = result.ApplyGraphQlArguments(context);

                        var filter = await GlobalFilters.GetFilter<TReturn>(context.UserContext, context.CancellationToken);
                        if (filter != null)
                        {
                            result = result.Where(filter);
                        }

                        return result;
                    })
            };
        }
    }
}