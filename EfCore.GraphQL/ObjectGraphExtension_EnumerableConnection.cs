using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace EfCoreGraphQL
{
    public static partial class ObjectGraphExtension
    {
        public static ConnectionBuilder<TGraphType, object> AddEnumerableConnectionField<TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturnType>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var connection = BuildEnumerableConnectionField<object, TGraphType, TReturnType>(name, resolve, includeName, pageSize);
            graphType.AddField(connection.FieldType);
            return connection;
        }

        public static ConnectionBuilder<TGraphType, TSourceType> AddEnumerableConnectionField<TSourceType, TGraphType, TReturnType>(
            this ObjectGraphType<TSourceType> graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var connection = BuildEnumerableConnectionField<TSourceType, TGraphType, TReturnType>(name, resolve, includeName, pageSize);
            graphType.AddField(connection.FieldType);
            return connection;
        }

        static ConnectionBuilder<TGraphType, TSourceType> BuildEnumerableConnectionField<TSourceType, TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName,
            int pageSize)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var builder = ConnectionBuilder.Create<TGraphType, TSourceType>();
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
                    Edges = BuildEdges(page, skip)
                };
            });
            return builder;
        }
    }
}