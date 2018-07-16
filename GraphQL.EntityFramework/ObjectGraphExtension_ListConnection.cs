using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

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
            builder.Name(name);
            builder.AddWhereArgument();
            builder.FieldType.SetIncludeMetadata(includeName);
            builder.Resolve(context =>
            {
                var list = resolve(context).ToList();
                var totalCount = list.Count;
                var skip = context.First.GetValueOrDefault(0);
                var take = context.PageSize.GetValueOrDefault(pageSize);
                var page = list.Skip(skip).Take(take);

                page = page.ApplyGraphQlArguments(context);

                return new Connection<TReturn>
                {
                    TotalCount = totalCount,
                    PageInfo = new PageInfo
                    {
                        HasNextPage = true,
                        HasPreviousPage = false,
                        StartCursor = skip.ToString(),
                        EndCursor = Math.Min(totalCount, skip + take).ToString(),
                    },
                    Edges = BuildEdges(page, skip)
                };
            });
            return builder;
        }
    }
}