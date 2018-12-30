using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService
    {
        FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class;

        FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class;

        FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class;

        FieldType AddQueryField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddQueryField<TSource, TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;

        FieldType AddQueryField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class;
    }
}