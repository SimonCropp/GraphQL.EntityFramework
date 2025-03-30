namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
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
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
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
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
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
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddSingleField<TReturn>(
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
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddSingleField<TSource, TReturn>(
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
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddSingleField<TSource, TReturn>(
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
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildSingleField<TSource, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate,
        Type? graphType,
        bool nullable,
        bool omitQueryArguments,
        bool idOnly)
        where TReturn : class
        => BuildSingleField(
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

    FieldType BuildSingleField<TSource, TReturn>(
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
            Resolver = new FuncFieldResolver<TSource, TReturn?>(
                async context =>
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

                    TReturn? single;
                    try
                    {
                        if (disableAsync)
                        {
                            single = query.SingleOrDefault();
                        }
                        else
                        {
                            single = await query.SingleOrDefaultAsync(context.CancellationToken);
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

                    if (single is not null)
                    {
                        if (efFieldContext.Filters == null ||
                            await efFieldContext.Filters.ShouldInclude(context.UserContext, efFieldContext.DbContext, context.User, single))
                        {
                            if (mutate is not null)
                            {
                                await mutate.Invoke(efFieldContext, single);
                            }

                            return single;
                        }
                    }

                    if (nullable)
                    {
                        return null;
                    }

                    throw new SingleEntityNotFoundException();
                })
        };

        if (!omitQueryArguments)
        {
            type.Arguments = ArgumentAppender.GetQueryArguments(hasId, false, idOnly, omitQueryArguments);
        }

        return type;
    }
}