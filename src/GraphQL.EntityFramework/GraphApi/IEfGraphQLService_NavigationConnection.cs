using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
    {
        void AddNavigationConnectionField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class;

        void AddNavigationConnectionField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class;
    }
}