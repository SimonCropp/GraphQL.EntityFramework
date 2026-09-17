namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        IObjectGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class
    {
        var field = BuildFirstField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        IObjectGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class
    {
        var field = BuildFirstField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class
    {
        var field = BuildFirstField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class
    {
        var field = BuildFirstField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddFirstField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class
    {
        var field = BuildFirstField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddFirstField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TReturn : class
    {
        var field = BuildFirstField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildFirstField<TSource, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate,
        Type? graphType,
        bool nullable,
        bool omitQueryArguments,
        bool idOnly)
        where TReturn : class
        => BuildFirstField(
            name,
            _ =>
            {
                var queryable = resolve(_);
                return Task.FromResult(queryable);
            },
            mutate,
            graphType,
            nullable,
            omitQueryArguments,
            idOnly);

    FieldType BuildFirstField<TSource, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>?>?> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate,
        Type? graphType,
        bool nullable,
        bool omitQueryArguments,
        bool idOnly)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>(nullable);

        var names = GetKeyNames<TReturn>();
        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var type = new FieldType
        {
            Name = name,
            Type = graphType,
            // object rather than TReturn, since a batch filter makes the value a deferred result
            Resolver = new FuncFieldResolver<TSource, object>(
                async context =>
            {
                var fieldContext = BuildContext(context);

                var task = resolve(fieldContext);
                if (task == null)
                {
                    return ReturnNullable();
                }

                var query = await task;
                if (query == null)
                {
                    return ReturnNullable();
                }

                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                query = query.ApplyGraphQlArguments(context, names, false, omitQueryArguments);

                query = includeAppender.ApplyProjection(context, fieldContext.Filters, query);

                QueryLogger.Write(query);

                TReturn? first;
                try
                {
                    if (query.Provider is IAsyncQueryProvider)
                    {
                        first = await query.FirstOrDefaultAsync(context.CancellationToken);
                    }
                    else
                    {
                        first = query.FirstOrDefault();
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
                         GraphType: {graphType.FullName}
                         TSource: {typeof(TSource).FullName}
                         TReturn: {typeof(TReturn).FullName}
                         OmitQueryArguments: {omitQueryArguments}
                         Nullable: {nullable}
                         KeyNames: {JoinKeys(names)}
                         Query: {QueryText(query)}
                         """,
                        exception);
                }

                if (first is null)
                {
                    return ReturnNullable(query);
                }

                if (fieldContext.Filters == null)
                {
                    return await Complete(first);
                }

                return await fieldContext.Filters.Apply(
                    context,
                    fieldContext.DbContext,
                    [first],
                    async included =>
                    {
                        if (included.Count == 0)
                        {
                            return ReturnNullable(query);
                        }

                        return await Complete(first);
                    });

                async ValueTask<object?> Complete(TReturn item)
                {
                    if (mutate is not null)
                    {
                        await mutate.Invoke(fieldContext, item);
                    }

                    return item;
                }
            })
        };

        if (!omitQueryArguments)
        {
            type.Arguments = ArgumentAppender.GetQueryArguments(typeof(TReturn), hasId, false, idOnly, omitQueryArguments);
        }

        return type;

        TReturn? ReturnNullable(IQueryable<TReturn>? query = null) =>
            nullable ? null : throw new FirstEntityNotFoundException(NotFoundQueryText(query));
    }
}
