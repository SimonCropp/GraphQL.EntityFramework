using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService
    {
        public FieldType AddNavigationField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildNavigationField(name, resolve, includeName, typeof(TGraph), arguments);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildNavigationField(name, resolve, includeName, graphType, arguments);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildNavigationField(name, resolve, includeName, graphType, arguments);
            return graph.AddField(field);
        }

        public FieldType AddNavigationField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildNavigationField(name, resolve, includeName, typeof(TGraph), arguments);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            string includeName,
            Type graphType,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            return new FieldType
            {
                Name = name,
                Type = graphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Metadata = IncludeAppender.GetIncludeMetadata(includeName),
                Resolver = new FuncFieldResolver<TSource, TReturn>(
                    context =>
                    {
                        return ExecuteWrapper.ExecuteQuery(name, graphType, context.Errors, () => resolve(context));
                    })
            };
        }
    }
}