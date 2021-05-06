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
        public QueryGraphType(IEfGraphQLService<TDbContext> graphQlService)
        {
            Guard.AgainstNull(nameof(graphQlService), graphQlService);
            GraphQlService = graphQlService;
        }

        public IEfGraphQLService<TDbContext> GraphQlService { get; }

        public TDbContext ResolveDbContext<TSource>(IResolveFieldContext<TSource> context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }

        public TDbContext ResolveDbContext(IResolveFieldContext context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }

        public void AddQueryConnectionField<TReturn>(
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


        public void AddQueryPaginationField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int page = 1,
            int row = 50,
            string? description = null)
            where TReturn : class
        {
            GraphQlService.AddQueryPaginationField(this, name, resolve, graphType, arguments, page, row, description);
        }

        public FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null,
            bool disableDefaultArguments = false)
            where TReturn : class
        {
            return GraphQlService.AddQueryField(this, name, resolve, graphType, arguments, description, disableDefaultArguments);
        }

        public FieldType AddUnionQueryField<TUnion>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IDictionary<Type, IQueryable<object>>> resolve,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TUnion : UnionGraphType
        {
            return GraphQlService.AddUnionQueryField<TUnion>(this, name, resolve, arguments, description);
        }

        
        public FieldType AddUnionQueryField<TUnion>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IDictionary<Type, IQueryable<object>>>> resolveAsync,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TUnion : UnionGraphType
        {
            return GraphQlService.AddUnionQueryField<TUnion>(this, name, resolveAsync, arguments, description);
        }

        public FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolveAsync,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null,
            bool disableDefaultArguments = false)
            where TReturn : class
        {
            return GraphQlService.AddQueryField(this, name, resolveAsync, graphType, arguments, description,disableDefaultArguments);
        }

        public FieldType AddSingleField<TReturn>(
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
            Type? graphType = null,
            string name = nameof(TReturn),
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false,
            string? description = null,
            bool disableDefaultArguments = false)
            where TReturn : class
        {
            return GraphQlService.AddSingleField(this, name, resolve, mutate, graphType, arguments, nullable, description, disableDefaultArguments);
        }

        public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
            where TItem : class
        {
            return GraphQlService.AddIncludes(query, context);
        }
    }
}