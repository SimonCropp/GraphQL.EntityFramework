using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
    {
        FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext,object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class;

        FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext,TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class;

        FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext,TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class;
    }
}