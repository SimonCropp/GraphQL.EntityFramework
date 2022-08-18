﻿using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        int pageSize = 10)
        where TReturn : class;

    ConnectionBuilder<TSource> AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        int pageSize = 10)
        where TReturn : class;
}