using System;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace EfCoreGraphQL
{
    public static partial class ObjectGraphExtension
    {
        public static ConnectionBuilder<TGraphType, object> AddQueryConnectionField<TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturnType>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraphType : ObjectGraphType<TReturnType>, IGraphType
            where TReturnType : class
        {
            var connection = BuildQueryConnectionField<object, TGraphType, TReturnType>(name, resolve, includeName, pageSize);
            graphType.AddField(connection.FieldType);
            return connection;
        }

        public static ConnectionBuilder<TGraphType, TSourceType> AddQueryConnectionField<TSourceType, TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IQueryable<TReturnType>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraphType : ObjectGraphType<TReturnType>, IGraphType
            where TReturnType : class
        {
            var connection = BuildQueryConnectionField<TSourceType, TGraphType, TReturnType>(name, resolve, includeName, pageSize);
            graphType.AddField(connection.FieldType);
            return connection;
        }

        static ConnectionBuilder<TGraphType, TSourceType> BuildQueryConnectionField<TSourceType, TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IQueryable<TReturnType>> resolve,
            string includeName,
            int pageSize)
            where TGraphType : ObjectGraphType<TReturnType>, IGraphType
            where TReturnType : class
        {
            var builder = ConnectionBuilder.Create<TGraphType, TSourceType>();
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
                return new Connection<TReturnType>
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