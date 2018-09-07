using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TSource> : ObjectGraphType<TSource>
    {
        IEfGraphQLService efGraphQlService;

        public EfObjectGraphType(IEfGraphQLService efGraphQlService)
        {
            Guard.AgainstNull(nameof(efGraphQlService), efGraphQlService);
            this.efGraphQlService = efGraphQlService;
        }

        protected ConnectionBuilder<TGraph, TSource> AddNavigationConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddNavigationConnectionField<TSource, TGraph, TReturn>(this, name, resolve, arguments, includeNames, pageSize, filter);
        }

        protected FieldType AddNavigationField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField<TSource, TGraph, TReturn>(this, name, resolve, arguments, includeNames);
        }

        protected FieldType AddNavigationField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, graphType, name, resolve, arguments, includeNames);
        }

        protected FieldType AddNavigationField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField<TSource, TGraph, TReturn>(this, name, resolve, arguments, includeNames, filter);
        }

        protected FieldType AddNavigationField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, graphType, name, resolve, arguments, includeNames, filter);
        }

        protected ConnectionBuilder<TGraph, TSource> AddQueryConnectionField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddQueryConnectionField<TSource, TGraph, TReturn>(this, name, resolve, arguments, pageSize, filter);
        }

        protected FieldType AddQueryField<TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TGraph : ObjectGraphType<TReturn>
            where TReturn : class
        {
            return efGraphQlService.AddQueryField<TSource, TGraph, TReturn>(this, name, resolve, arguments, filter);
        }

        protected FieldType AddQueryField<TReturn>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            Func<IEnumerable<TReturn>, IEnumerable<TReturn>> filter = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, graphType, name, resolve, arguments, filter);
        }
    }
}