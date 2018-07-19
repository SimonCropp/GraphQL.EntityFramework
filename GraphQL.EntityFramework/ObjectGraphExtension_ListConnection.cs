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
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildListConnectionField<object, TGraph, TReturn>(efGraphQlService, name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        public static ConnectionBuilder<TGraph, TSource> AddListConnectionField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildListConnectionField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        static ConnectionBuilder<TGraph, TSource> BuildListConnectionField<TSource, TGraph, TReturn>(
            EfGraphQLService efGraphQlService,
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
            IncludeAppender.SetIncludeMetadata(builder.FieldType, includeName);
            builder.Resolve(context =>
            {
                return ExecuteQuery(name, typeof(TGraph), context.Errors, () =>
                {
                    var enumerable = resolve(context);
                    var page = enumerable
                        .ApplyGraphQlArguments(context)
                        .ToList();

                    return ConnectionConverter.ApplyConnectionContext(
                        page,
                        context.First,
                        context.After,
                        context.Last,
                        context.Before);
                });
            });
            return builder;
        }
    }
}