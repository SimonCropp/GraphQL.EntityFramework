﻿using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework;

//Navigation fields will always be on a typed graph. so use ComplexGraphType not IComplexGraphType
public partial interface IEfGraphQLService<TDbContext>
{
    FieldType AddNavigationField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve = null,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddNavigationListField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null)
        where TReturn : class;
}