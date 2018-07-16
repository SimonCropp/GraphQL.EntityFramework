using System;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

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

        static ConnectionBuilder<TGraph, TSource> BuildQueryConnectionField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName,
            int pageSize)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var builder = ConnectionBuilder.Create<TGraph, TSource>();
            builder.Name(name);
            builder.AddWhereArgument();
            builder.FieldType.SetIncludeMetadata(includeName);
            builder.ResolveAsync(async context =>
            {
                var list = resolve(context);
                var totalCount = await list.CountAsync().ConfigureAwait(false);
                var skip = context.First.GetValueOrDefault(0);
                var take = context.PageSize.GetValueOrDefault(pageSize);
                var page = list.Skip(skip).Take(take);

                page = IncludeAppender.AddIncludes(page, context)
                    .ApplyGraphQlArguments(context);

                var result = await page
                    .ToListAsync()
                    .ConfigureAwait(false);
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
                    Edges = BuildEdges(result, skip)
                };
            });
            return builder;
        }
    }
}