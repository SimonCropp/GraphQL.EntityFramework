using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public abstract class QueryGraphType<TDbContext> :
        ObjectGraphType
        where TDbContext : DbContext
    {
        protected QueryGraphType(IEfGraphQLService<TDbContext> graphQlService)
        {
            Guard.AgainstNull(nameof(graphQlService), graphQlService);
            GraphQlService = graphQlService;
        }

        public IEfGraphQLService<TDbContext> GraphQlService { get; }

        protected TDbContext ResolveDbContext<TSource>(IResolveFieldContext<TSource> context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }

        public TDbContext ResolveDbContext(IResolveFieldContext context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }

        protected void AddQueryConnectionField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            GraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize, description);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return GraphQlService.AddQueryField(this, name, resolve, graphType, arguments, description);
        }

        protected FieldType AddSingleField<TReturn>(
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
            Type? graphType = null,
            string name = nameof(TReturn),
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false,
            string? description = null)
            where TReturn : class
        {
            return GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, arguments, nullable, description);
        }

        protected IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
            where TItem : class
        {
            return GraphQlService.AddIncludes(query, context);
        }
    }
}