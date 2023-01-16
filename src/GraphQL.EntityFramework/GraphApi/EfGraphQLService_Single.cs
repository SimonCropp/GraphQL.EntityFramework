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
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments);
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
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments);
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
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildSingleField(name, resolve, mutate, graphType, nullable, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildSingleField<TSource, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate,
        Type? graphType,
        bool nullable,
        bool omitQueryArguments)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>(nullable);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var type = new FieldType
        {
            Name = name,
            Type = graphType,
            Resolver = new FuncFieldResolver<TSource, TReturn?>(
                async context =>
                {
                    var efFieldContext = BuildContext(context);

                    var names = GetKeyNames<TReturn>();

                    var query = resolve(efFieldContext);
                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    if (!omitQueryArguments)
                    {
                        query = query.ApplyGraphQlArguments(context, names, false);
                    }

                    QueryLogger.Write(query);

                    TReturn? single;
                    if (disableAsync)
                    {
                        single = query.SingleOrDefault();
                    }
                    else
                    {
                        single = await query.SingleOrDefaultAsync(context.CancellationToken);
                    }

                    if (single is not null)
                    {
                        if (await efFieldContext.Filters.ShouldInclude(context.UserContext, context.User, single))
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

                    throw new ExecutionError("Not found");
                })
        };
        if (!omitQueryArguments)
        {
            type.Arguments = ArgumentAppender.GetQueryArguments(hasId, false);
        }
        return type;
    }
}