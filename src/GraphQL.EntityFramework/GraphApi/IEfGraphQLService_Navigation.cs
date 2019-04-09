using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public partial interface IEfGraphQLService
    {
        FieldType AddNavigationField<TSource, TReturn>(ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, TReturn> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class;

        FieldType AddNavigationField<TReturn>(ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, TReturn> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class;

        FieldType AddNavigationListField<TSource, TReturn>(ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class;

        FieldType AddNavigationListField<TReturn>(ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null)
            where TReturn : class;
    }
}