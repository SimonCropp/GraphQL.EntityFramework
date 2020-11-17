using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
    {
        void AddQueryPaginationField<TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int page = 1,
            int row = 50,
            string? description = null)
            where TReturn : class;

        void AddQueryPaginationField<TSource, TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int page = 1,
            int row = 50,
            string? description = null)
            where TReturn : class;
    }
}