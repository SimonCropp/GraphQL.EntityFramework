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
            int pageSize = 10)
            where TReturn : class
        {
            GraphQlService.AddNavigationConnectionField<TSource, TReturn>(this, name, graphType, arguments, includeNames, pageSize);
        }

        public FieldType AddNavigationField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null)
            where TReturn : class
        {
            return GraphQlService.AddNavigationField<TSource, TReturn>(this, name, graphType, includeNames);
        }

        public FieldType AddNavigationListField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null)
            where TReturn : class
        {
            return GraphQlService.AddNavigationListField<TSource, TReturn>(this, name, graphType, arguments, includeNames);
        }

        public void AddQueryConnectionField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            GraphQlService.AddQueryConnectionField<TSource, TReturn>(this, name, graphType, arguments, pageSize);
        }

        public FieldType AddQueryField<TReturn>(
            string name,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return GraphQlService.AddQueryField<TSource, TReturn>(this, name, graphType, arguments);
        }

        public TDbContext ResolveDbContext(ResolveFieldContext<TSource> context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }

        public TDbContext ResolveDbContext(ResolveFieldContext context)
        {
            Guard.AgainstNull(nameof(context), context);
            return GraphQlService.ResolveDbContext(context);
        }
    }
}