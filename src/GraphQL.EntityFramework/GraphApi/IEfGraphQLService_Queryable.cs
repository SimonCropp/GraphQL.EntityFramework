﻿namespace GraphQL.EntityFramework;

public partial interface IEfGraphQLService<TDbContext>
{
    FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>, IOrderedQueryable<TReturn>>? orderBy = null)
        where TReturn : class;

    FieldBuilder<TSource, TReturn> AddQueryField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>, IOrderedQueryable<TReturn>>? orderBy = null)
        where TReturn : class;
}