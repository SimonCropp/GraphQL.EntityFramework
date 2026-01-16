namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    #region AddProjectedFirstField - TSource generic variants

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
            name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (_, proj) => Task.FromResult(transform(proj)),
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
            name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (_, proj) => transform(proj),
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
            name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (ctx, proj) => Task.FromResult(transform(ctx, proj)),
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField(
            name,
            context => Task.FromResult(resolve(context)),
            projection,
            transform,
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
            name,
            resolve,
            projection,
            (_, proj) => Task.FromResult(transform(proj)),
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
            name,
            resolve,
            projection,
            (_, proj) => transform(proj),
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
            name,
            resolve,
            projection,
            (ctx, proj) => Task.FromResult(transform(ctx, proj)),
            graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedFirstField(name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    #endregion

    #region AddProjectedFirstField - object source variants

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false,
        bool omitQueryArguments = false,
        bool idOnly = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedFirstField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, graphType, nullable, omitQueryArguments, idOnly);

    #endregion

    #region BuildProjectedFirstField

    FieldType BuildProjectedFirstField<TSource, TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType,
        bool nullable,
        bool omitQueryArguments,
        bool idOnly)
        where TEntity : class
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>(nullable);

        var compiledProjection = projection.Compile();

        // Extract navigation includes from projection
        var autoIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);

        var names = GetKeyNames<TEntity>();
        var hasId = keyNames.ContainsKey(typeof(TEntity));

        var fieldType = new FieldType
        {
            Name = name,
            Type = graphType
        };

        // Set metadata for auto-detected includes
        if (autoIncludes.Count > 0)
        {
            IncludeAppender.SetIncludeMetadata(fieldType, name, autoIncludes);
        }

        if (!omitQueryArguments)
        {
            fieldType.Arguments = ArgumentAppender.GetQueryArguments(hasId, false, idOnly, omitQueryArguments);
        }

        fieldType.Resolver = new FuncFieldResolver<TSource, TReturn?>(
            async context =>
            {
                var fieldContext = BuildContext(context);

                var task = resolve(fieldContext);
                if (task is null)
                {
                    return ReturnNullable();
                }

                var query = await task;
                if (query is null)
                {
                    return ReturnNullable();
                }

                try
                {
                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    query = query.ApplyGraphQlArguments(context, names, false, omitQueryArguments);

                    QueryLogger.Write(query);

                    TEntity? entity;
                    if (query.Provider is IAsyncQueryProvider)
                    {
                        entity = await query.FirstOrDefaultAsync(context.CancellationToken);
                    }
                    else
                    {
                        entity = query.FirstOrDefault();
                    }

                    if (entity is null)
                    {
                        return ReturnNullable(query);
                    }

                    if (fieldContext.Filters is not null &&
                        !await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, entity))
                    {
                        return ReturnNullable(query);
                    }

                    var projectedData = compiledProjection(entity);
                    var result = await transform(fieldContext, projectedData);

                    return result;
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
                         Failed to execute projected first field `{name}`
                         GraphType: {graphType.FullName}
                         TSource: {typeof(TSource).FullName}
                         TEntity: {typeof(TEntity).FullName}
                         TProjection: {typeof(TProjection).FullName}
                         TReturn: {typeof(TReturn).FullName}
                         Query: {query.SafeToQueryString()}
                         """,
                        exception);
                }

                TReturn? ReturnNullable(IQueryable<TEntity>? q = null) =>
                    nullable ? null : throw new FirstEntityNotFoundException(q?.SafeToQueryString());
            });

        return fieldType;
    }

    #endregion
}
