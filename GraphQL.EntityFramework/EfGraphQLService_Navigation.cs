using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public FieldType AddNavigationField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Filter<object, TReturn> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(name, resolve, includeNames, typeof(TGraph), arguments, filter);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Filter<TSource, TReturn> filter = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(name, resolve, includeNames, graphType, arguments, filter);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Filter<object, TReturn> filter = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(name, resolve, includeNames, graphType, arguments, filter);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Filter<TSource, TReturn> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildNavigationField(name, resolve, includeNames, typeof(TGraph), arguments, filter);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<string> includeNames,
            Type graphType,
            IEnumerable<QueryArgument> arguments,
            Filter<TSource, TReturn> filter)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(graphType), graphType);
            Guard.AgainstNull(nameof(resolve), resolve);
            return new FieldType
            {
                Name = name,
                Type = graphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames),
                Resolver = new FuncFieldResolver<TSource, TReturn>(context =>
                {
                    var result = resolve(context);
                    if (filter != null)
                    {
                        if (!filter(context, result))
                        {
                            return null;
                        }
                    }

                    if (!GlobalFilters.ShouldInclude(context.UserContext, result))
                    {
                        return null;
                    }

                    return result;
                })
            };
        }
    }
}