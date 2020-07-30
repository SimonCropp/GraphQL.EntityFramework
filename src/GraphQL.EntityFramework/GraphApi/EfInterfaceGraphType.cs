using System;
using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    public class EfInterfaceGraphType<TDbContext, TSource> :
        InterfaceGraphType<TSource>
        where TDbContext : DbContext
    {
        public IEfGraphQLService<TDbContext> GraphQlService { get; }

        public EfInterfaceGraphType(IEfGraphQLService<TDbContext> graphQlService)
        {
            Guard.AgainstNull(nameof(graphQlService), graphQlService);
            GraphQlService = graphQlService;
        }

        public void AddNavigationConnectionField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            GraphQlService.AddNavigationConnectionField<TSource, TReturn>(this, name, graphType, arguments, includeNames, pageSize, description);
        }

        public FieldType AddNavigationField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            return GraphQlService.AddNavigationField<TSource, TReturn>(this, name, graphType, includeNames, description);
        }

        public FieldType AddNavigationListField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            return GraphQlService.AddNavigationListField<TSource, TReturn>(this, name, graphType, arguments, includeNames, description);
        }

        public void AddQueryConnectionField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            GraphQlService.AddQueryConnectionField<TSource, TReturn>(this, name, graphType, arguments, pageSize, description);
        }

        public FieldType AddQueryField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return GraphQlService.AddQueryField<TSource, TReturn>(this, name, graphType, arguments, description);
        }

        public TDbContext ResolveDbContext(IResolveFieldContext<TSource> context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }

        public TDbContext ResolveDbContext(IResolveFieldContext context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }
    }
}