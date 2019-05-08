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
            int pageSize = 10,
            string primaryKeyName = "Id")
            where TReturn : class
        {
            efGraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize, primaryKeyName);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            string primaryKeyName = "Id")
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name, resolve, graphType, arguments, primaryKeyName);
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