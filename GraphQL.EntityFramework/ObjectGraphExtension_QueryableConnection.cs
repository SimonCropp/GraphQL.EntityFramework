using System;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        public static ConnectionBuilder<TGraph, object> AddQueryConnectionField<TGraph, TReturn>(
            this ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildQueryConnectionField<object, TGraph, TReturn>(name, resolve, includeName, pageSize);
            graph.AddField(connection.FieldType);
            return connection;
        }

        public static ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TSource, TGraph, TReturn>(
            this ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildQueryConnectionField<TSource, TGraph, TReturn>(name, resolve, includeName, pageSize);
            graph.AddField(connection.FieldType);
            return connection;
        }

        public static ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TSource, TGraph, TReturn>(
            this ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildQueryConnectionField<TSource, TGraph, TReturn>(name, resolve, includeName, pageSize);
            graph.AddField(connection.FieldType);
            return connection;
        }

        static ConnectionBuilder<TGraph, TSource> BuildQueryConnectionField<TSource, TGraph, TReturn>(
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
            builder.AddWhereArgument();
            builder.FieldType.SetIncludeMetadata(includeName);
            builder.Resolve(connectionContext =>
            {
                var list = resolve(connectionContext);
                list = IncludeAppender.AddIncludes(list, connectionContext)
                    .ApplyGraphQlArguments(connectionContext);
                return ConnectionConverter.ApplyConnectionContext(
                    list,
                    connectionContext.First,
                    connectionContext.After,
                    connectionContext.Last,
                    connectionContext.Before);
            });
            return builder;
        }
    }
}