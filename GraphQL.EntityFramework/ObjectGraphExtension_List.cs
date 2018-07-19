using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        public static FieldType AddListField<TGraph, TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildListField<object, TGraph, TReturn>(efGraphQlService, name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        public static FieldType AddListField<TSource, TReturn>(
            this ObjectGraphType<TSource> graph,
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildListField(efGraphQlService, graphType, name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        public static FieldType AddListField<TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildListField(efGraphQlService, graphType, name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        static FieldType BuildListField<TSource, TReturn>(
            EfGraphQLService efGraphQlService,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(graphType);
            return BuildListField(efGraphQlService, name, resolve, includeName, listGraphType, arguments);
        }

        public static FieldType AddListField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildListField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        static FieldType BuildListField<TSource, TGraph, TReturn>(
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            IEnumerable<QueryArgument> arguments)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var listGraphType = typeof(ListGraphType<TGraph>);
            return BuildListField(efGraphQlService, name, resolve, includeName, listGraphType, arguments);
        }

        static FieldType BuildListField<TSource, TReturn>(
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            Type listGraphType,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Metadata = IncludeAppender.GetIncludeMetadata(includeName),
                Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
                    context =>
                    {
                        return ExecuteQuery(name, listGraphType, context.Errors, () =>
                        {
                            var returnTypes = resolve(context);
                            return returnTypes
                                .ApplyGraphQlArguments(context);
                        });
                    })
            };
        }
    }
}