using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TSource> : ObjectGraphType<TSource>
    {
        EfGraphQLService efGraphQlService;

        public EfObjectGraphType(EfGraphQLService efGraphQlService)
        {
            this.efGraphQlService = efGraphQlService;
        }
        protected ConnectionBuilder<TGraph, TSource> AddListConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddListConnectionField<TSource, TGraph, TReturn>(efGraphQlService,name, resolve, arguments, includeName, pageSize);
        }

        protected FieldType AddListField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddListField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, arguments, includeName);
        }

        protected FieldType AddListField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            return this.AddListField(efGraphQlService, graphType, name, resolve, arguments, includeName);
        }

        protected ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddQueryConnectionField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, arguments, includeName, pageSize);
        }

        protected FieldType AddQueryField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return this.AddQueryField<TSource, TGraph, TReturn>(efGraphQlService, name, resolve, arguments, includeName);
        }

        protected FieldType AddQueryField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null)
            where TReturn : class
        {
            return this.AddQueryField(efGraphQlService, graphType, name, resolve, arguments, includeName);
        }
    }
}