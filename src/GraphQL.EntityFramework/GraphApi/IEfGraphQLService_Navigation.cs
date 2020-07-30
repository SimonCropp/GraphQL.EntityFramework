using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService<TDbContext>
    {
        FieldType AddNavigationField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?> resolve,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class;

        FieldType AddNavigationField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class;

        FieldType AddNavigationListField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class;

        FieldType AddNavigationListField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class;
    }
}