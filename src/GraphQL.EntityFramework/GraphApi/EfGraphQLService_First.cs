namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<object, TReturn> AddFirstField<TReturn>(
        IObjectGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
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
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate,
        Type? graphType,
        bool nullable,
        bool omitQueryArguments,
        bool idOnly)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>(nullable);

        var names = GetKeyNames<TReturn>();
        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var type = new FieldType
        {
            Name = name,
            Type = graphType,
            Resolver = new FuncFieldResolver<TSource, TReturn?>(async context =>
            {
                var efFieldContext = BuildContext(context);

                var query = await resolve(efFieldContext);
                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                query = includeAppender.AddIncludes(query, context);
                query = query.ApplyGraphQlArguments(context, names, false, omitQueryArguments);

                QueryLogger.Write(query);

                TReturn? first;
                try
                {
                    if (disableAsync)
                    {
                        first = query.FirstOrDefault();
                    }
                    else
                    {
                        first = await query.FirstOrDefaultAsync(context.CancellationToken);
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
                         DisableAsync: {disableAsync}
                         OmitQueryArguments: {omitQueryArguments}
                         Nullable: {nullable}
                         KeyNames: {JoinKeys(names)}
                         Query: {query.ToQueryString()}
                         """,
                        exception);
                }

                if (first is not null)
                {
                    if (efFieldContext.Filters == null ||
                        await efFieldContext.Filters.ShouldInclude(context.UserContext, efFieldContext.DbContext, context.User, first))
                    {
                        if (mutate is not null)
                        {
                            await mutate.Invoke(efFieldContext, first);
                        }

                        return first;
                    }
                }

                if (nullable)
                {
                    return null;
                }

                throw new FirstEntityNotFoundException();
            })
        };

        if (!omitQueryArguments)
        {
            type.Arguments = ArgumentAppender.GetQueryArguments(hasId, false, idOnly, omitQueryArguments);
        }

        return type;
    }
}