using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        public static ConnectionBuilder<TGraph, object> AddListConnectionField<TGraph, TReturn>(
            this ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildListConnectionField<object, TGraph, TReturn>(name, resolve, includeName, pageSize);
            graph.AddField(connection.FieldType);
            return connection;
        }

        public static ConnectionBuilder<TGraph, TSource> AddListConnectionField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildListConnectionField<TSource, TGraph, TReturn>(name, resolve, includeName, pageSize);
            graph.AddField(connection.FieldType);
            return connection;
        }

        static ConnectionBuilder<TGraph, TSource> BuildListConnectionField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            int pageSize)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var builder = ConnectionBuilder.Create<TGraph, TSource>();
            builder.PageSize(pageSize);
            builder.Name(name);
            builder.AddWhereArgument();
            builder.FieldType.SetIncludeMetadata(includeName);
            builder.Resolve(context =>
            {
                var enumerable = resolve(context);
                try
                {
                    var page = enumerable
                        .ApplyGraphQlArguments(context)
                        .ToList();

                    return ConnectionConverter.ApplyConnectionContext(
                        page,
                        context.First,
                        context.After,
                        context.Last,
                        context.Before);
                }
                catch (ErrorException exception)
                {
                    context.Errors.Add(new ExecutionError(exception.Message));
                    throw;
                }
            });
            return builder;
        }
    }
}