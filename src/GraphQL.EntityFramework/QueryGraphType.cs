using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class QueryGraphType :
        ObjectGraphType
    {
        IEfGraphQLService efGraphQlService;

        public QueryGraphType(IEfGraphQLService efGraphQlService)
        {
            Guard.AgainstNull(nameof(efGraphQlService), efGraphQlService);
            this.efGraphQlService = efGraphQlService;
        }

        protected void AddNavigationConnectionField<TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
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
            Func<ResolveFieldContext<object>, TReturn> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, name, resolve, graphType, arguments, includeNames);
        }

        protected FieldType AddNavigationListField<TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationListField(this, name, resolve, graphType, arguments, includeNames);
        }

        protected void AddQueryConnectionField<TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            efGraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name, resolve, graphType, arguments);
        }

        protected FieldType AddSingleField<TReturn>(
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            string name = nameof(TReturn))
            where TReturn : class
        {
            return efGraphQlService.AddSingleField(this, name, resolve, graphType);
        }
    }
}