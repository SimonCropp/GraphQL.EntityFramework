using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public class QueryGraphType<TDbContext> :
        ObjectGraphType
        where TDbContext : DbContext
    {
        IEfGraphQLService<TDbContext> efGraphQlService;

        public QueryGraphType(IEfGraphQLService<TDbContext> efGraphQlService)
        {
            Guard.AgainstNull(nameof(efGraphQlService), efGraphQlService);
            this.efGraphQlService = efGraphQlService;
        }

        public TDbContext ResolveDbContext<TSource>(IResolveFieldContext<TSource> context)
        {
            Guard.AgainstNull(nameof(context), context);
            return efGraphQlService.ResolveDbContext(context);
        }

        // public TDbContext ResolveDbContext(IResolveFieldContext context)
        // {
        //     Guard.AgainstNull(nameof(context), context);
        //     return efGraphQlService.ResolveDbContext<object>(context);
        // }

        protected void AddQueryConnectionField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            efGraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name, resolve, graphType, arguments);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name, resolve, graphType, arguments);
        }

        protected FieldType AddSingleField<TReturn>(
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            string name = nameof(TReturn),
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return efGraphQlService.AddSingleField(this, name, resolve, graphType, arguments, nullable);
        }

        protected FieldType AddSingleField<TReturn>(
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            string name = nameof(TReturn),
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return efGraphQlService.AddSingleField(this, name, resolve, graphType, arguments, nullable);
        }
    }
}