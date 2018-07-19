using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TSource> : ObjectGraphType<TSource>
    {
        protected ConnectionBuilder<TGraph, TSource> AddListConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddListConnectionField<TSource, TGraph, TReturn>(name, resolve, includeName, pageSize);
        }

        protected FieldType AddListField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddListField<TSource, TGraph, TReturn>(name, resolve, includeName);
        }

        protected FieldType AddListField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName = null)
            where TReturn : class
        {
            return ObjectGraphExtension.AddListField(this, graphType, name, resolve, includeName);
        }

        protected ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddQueryConnectionField<TSource, TGraph, TReturn>(name, resolve, includeName, pageSize);
        }

        protected FieldType AddQueryField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddQueryField<TSource, TGraph, TReturn>(name, resolve, includeName);
        }

        protected FieldType AddQueryField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            string includeName = null)
            where TReturn : class
        {
            return ObjectGraphExtension.AddQueryField(this, graphType, name, resolve, includeName);
        }
    }
}