using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TSource> :
        ObjectGraphType<TSource>
    {
        IEfGraphQLService efGraphQlService;

        public EfObjectGraphType(IEfGraphQLService efGraphQlService)
        {
            Guard.AgainstNull(nameof(efGraphQlService), efGraphQlService);
            this.efGraphQlService = efGraphQlService;
        }

        protected void AddNavigationConnectionField<TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10)
            where TReturn : class
        {
            efGraphQlService.AddNavigationConnectionField(this, name, resolve, graphType, arguments, includeNames, pageSize);
        }

        protected FieldType AddNavigationField<TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, name, resolve, graphType, arguments, includeNames);
        }

        protected FieldType AddNavigationListField<TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationListField(this, name, resolve, graphType, arguments, includeNames);
        }

        protected void AddQueryConnectionField<TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            efGraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name, resolve, graphType, arguments);
        }
    }
}