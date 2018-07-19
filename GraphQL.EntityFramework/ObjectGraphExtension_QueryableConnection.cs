using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        public static ConnectionBuilder<TGraph, object> AddQueryConnectionField<TGraph, TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildQueryConnectionField<object, TGraph, TReturn>(efGraphQlService,name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        public static ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TSource, TGraph, TReturn>(
            this ObjectGraphType graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildQueryConnectionField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        public static ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildQueryConnectionField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        static ConnectionBuilder<TGraph, TSource> BuildQueryConnectionField<TSource, TGraph, TReturn>(
            EfGraphQLService efGraphQlService,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName,
            int pageSize)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var builder = ConnectionBuilder.Create<TGraph, TSource>();
            builder.PageSize(pageSize);
            //todo:
            //builder.Bidirectional();
            builder.Name(name);
            IncludeAppender.SetIncludeMetadata(builder.FieldType, includeName);
            builder.ResolveAsync(async context =>
            {
                return await ExecuteConnection(name, typeof(TGraph), context.Errors, () =>
                {
                    return efGraphQlService.IncludeAppender.AddIncludes(resolve(context),context)
                        .ApplyGraphQlArguments(context)
                        .ApplyConnectionContext(context.First,
                            context.After,
                            context.Last,
                            context.Before);
                });
            });
            return builder;
        }
    }
}