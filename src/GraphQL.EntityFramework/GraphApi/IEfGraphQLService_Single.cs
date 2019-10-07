using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
    {
        FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class;

        FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class;

        FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class;

        FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class;

        FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class;

        FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class;
    }
}