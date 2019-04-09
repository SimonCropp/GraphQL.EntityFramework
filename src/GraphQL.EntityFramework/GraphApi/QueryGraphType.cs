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