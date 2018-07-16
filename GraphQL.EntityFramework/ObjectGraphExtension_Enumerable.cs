using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        public static FieldType AddEnumerableField<TGraph, TReturn>(
            this ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildEnumerableField<object, TGraph, TReturn>(name, resolve, includeName);
            return graph.AddField(field);
        }

        public static FieldType AddEnumerableField<TSource, TReturn>(
            this ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildEnumerableField(graphType, name, resolve, includeName);
            return graph.AddField(field);
        }

        public static FieldType AddEnumerableField<TReturn>(
            this ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildEnumerableField(graphType, name, resolve, includeName);
            return graph.AddField(field);
        }

        static FieldType BuildEnumerableField<TSource, TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName)
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(graphType);
            return BuildEnumerableField(name, resolve, includeName, listGraphType);
        }

        public static FieldType AddEnumerableField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildEnumerableField<TSource, TGraph, TReturn>(name, resolve, includeName);
            return graph.AddField(field);
        }

        static FieldType BuildEnumerableField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var listGraphType = typeof(ListGraphType<TGraph>);
            return BuildEnumerableField(name, resolve, includeName, listGraphType);
        }

        static FieldType BuildEnumerableField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            Type listGraphType)
            where TReturn : class
        {
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(),
                Metadata = IncludeAppender.GetIncludeMetadata(includeName),
                Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
                    context =>
                    {
                        var returnTypes = resolve(context);
                        return returnTypes
                            .ApplyGraphQlArguments(context);
                    })
            };
        }
    }
}