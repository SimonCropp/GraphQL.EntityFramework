namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildQueryField(graphType, name, resolve, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>>? resolve = null,
        Type? graphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildQueryField(graphType, name, resolve, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddQueryField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildQueryField(itemGraphType, name, resolve, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddQueryField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildQueryField(itemGraphType, name, resolve, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildQueryField<TSource, TReturn>(
        Type? itemGraphType,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve,
        bool omitQueryArguments)
        where TReturn : class =>
        BuildQueryField<TSource, TReturn>(
            itemGraphType,
            name,
            resolve == null ? null : context => Task.FromResult(resolve(context)),
            omitQueryArguments);

    FieldType BuildQueryField<TSource, TReturn>(
        Type? itemGraphType,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>>? resolve,
        bool omitQueryArguments)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        var keyFunc = GetKeyFunc<TReturn>();
        var hasId = keys.ContainsKey(typeof(TReturn));
        var fieldType = new FieldType
        {
            Name = name,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(keyFunc, true, false, omitQueryArguments),
        };

        var names = GetKeyFunc<TReturn>();
        if (resolve is not null)
        {
            fieldType.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(async context =>
            {
                var fieldContext = BuildContext(context);
                var query = await resolve(fieldContext);
                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                query = includeAppender.AddIncludes(query, context);
                if (!omitQueryArguments)
                {
                    query = query.ApplyGraphQlArguments(context, names, true, omitQueryArguments);
                }

                QueryLogger.Write(query);

                List<TReturn> list;

                try
                {
                    if (disableAsync)
                    {
                        list = query.ToList();
                    }
                    else
                    {
                        list = await query
                            .ToListAsync(context.CancellationToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                         Failed to execute query for field `{name}`
                         GraphType: {fieldType.Type.FullName}
                         TSource: {typeof(TSource).FullName}
                         TReturn: {typeof(TReturn).FullName}
                         DisableTracking: {disableTracking}
                         HasId: {hasId}
                         DisableAsync: {disableAsync}
                         KeyNames: {JoinKeys<TReturn>()}
                         Query: {query.ToQueryString()}
                         """,
                        exception);
                }

                if (fieldContext.Filters == null)
                {
                    return list;
                }

                return await fieldContext.Filters.ApplyFilter(list, context.UserContext, fieldContext.DbContext, context.User);
            });
        }

        return fieldType;
    }

    static Type listGraphType = typeof(ListGraphType<>);
    static Type nonNullType = typeof(NonNullGraphType<>);

    static Type MakeListGraphType<TReturn>(Type? itemGraphType)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        return nonNullType.MakeGenericType(listGraphType.MakeGenericType(itemGraphType));
    }
}