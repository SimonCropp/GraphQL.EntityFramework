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
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildListField<object, TGraph, TReturn>(name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        public static FieldType AddListField<TSource, TReturn>(
            this ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildListField(graphType, name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        public static FieldType AddListField<TReturn>(
            this ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            var field = BuildListField(graphType, name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        static FieldType BuildListField<TSource, TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            IEnumerable<QueryArgument> arguments)
            where TReturn : class
        {
            var listGraphType = MakeListGraphType(graphType);
            return BuildListField(name, resolve, includeName, listGraphType, arguments);
        }

        public static FieldType AddListField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var field = BuildListField<TSource, TGraph, TReturn>(name, resolve, includeName, arguments);
            return graph.AddField(field);
        }

        static FieldType BuildListField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            IEnumerable<QueryArgument> arguments)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var listGraphType = typeof(ListGraphType<TGraph>);
            return BuildListField(name, resolve, includeName, listGraphType, arguments);
        }

        static FieldType BuildListField<TSource, TReturn>(
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
                        var returnTypes = resolve(context);
                        try
                        {
                            return returnTypes
                                .ApplyGraphQlArguments(context);
                        }
                        catch (ErrorException exception)
                        {
                            context.Errors.Add(new ExecutionError(exception.Message));
                            throw;
                        }
                    })
            };
        }
    }
}