using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TDbContext, TSource> :
        ObjectGraphType<TSource>
        where TDbContext : DbContext
    {
        IEfGraphQLService<TDbContext> efGraphQlService;

        public EfObjectGraphType(IEfGraphQLService<TDbContext> efGraphQlService)
        {
            Guard.AgainstNull(nameof(efGraphQlService), efGraphQlService);
            this.efGraphQlService = efGraphQlService;
        }

        protected void AddNavigationConnectionField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10)
            where TReturn : class
        {
            efGraphQlService.AddNavigationConnectionField(this, name, resolve, graphType, arguments, includeNames, pageSize);
        }

        protected FieldType AddNavigationField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?> resolve,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationField(this, name, resolve, graphType, includeNames);
        }

        protected FieldType AddNavigationListField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null)
            where TReturn : class
        {
            return efGraphQlService.AddNavigationListField(this, name, resolve, graphType, arguments, includeNames);
        }

        protected void AddQueryConnectionField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            efGraphQlService.AddQueryConnectionField(this, name, resolve, graphType, arguments, pageSize);
        }

        protected FieldType AddQueryField<TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return efGraphQlService.AddQueryField(this, name, resolve, graphType, arguments);
        }
    }
}