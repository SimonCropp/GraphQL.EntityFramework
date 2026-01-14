namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    #region AddProjectedNavigationField

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => Task.FromResult(transform(proj));
        var field = BuildProjectedNavigationField(name, resolve, projection, asyncTransform, graphType, includeNames);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => transform(proj);
        var field = BuildProjectedNavigationField(name, resolve, projection, asyncTransform, graphType, includeNames);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> ctx, TProjection proj) => Task.FromResult(transform(ctx, proj));
        var field = BuildProjectedNavigationField(name, resolve, projection, asyncTransform, graphType, includeNames);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        IEnumerable<string>? includeNames = null)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationField(name, resolve, projection, transform, graphType, includeNames);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildProjectedNavigationField<TSource, TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TEntity?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType,
        IEnumerable<string>? includeNames)
        where TEntity : class
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var compiledProjection = projection.Compile();

        var field = new FieldType
        {
            Name = name,
            Type = graphType
        };

        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        field.Resolver = new FuncFieldResolver<TSource, TReturn>(
            async context =>
            {
                var fieldContext = BuildContext(context);

                TEntity? entity;
                try
                {
                    entity = resolve(fieldContext);
                }
                catch (Exception exception)
                {
                    throw new(
                        $"""
                        Failed to execute navigation resolve for projected field `{name}`
                        GraphType: {graphType.FullName}
                        TSource: {typeof(TSource).FullName}
                        TEntity: {typeof(TEntity).FullName}
                        TProjection: {typeof(TProjection).FullName}
                        TReturn: {typeof(TReturn).FullName}
                        """,
                        exception);
                }

                if (entity is null)
                {
                    return default;
                }

                if (fieldContext.Filters is not null &&
                    !await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, entity))
                {
                    return default;
                }

                var projectedData = compiledProjection(entity);
                var result = await transform(fieldContext, projectedData);
                return result;
            });

        return field;
    }

    #endregion

    #region AddProjectedNavigationListField

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => Task.FromResult(transform(proj));
        var field = BuildProjectedNavigationListField(name, resolve, projection, asyncTransform, itemGraphType, includeNames, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => transform(proj);
        var field = BuildProjectedNavigationListField(name, resolve, projection, asyncTransform, itemGraphType, includeNames, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> ctx, TProjection proj) => Task.FromResult(transform(ctx, proj));
        var field = BuildProjectedNavigationListField(name, resolve, projection, asyncTransform, itemGraphType, includeNames, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationListField(name, resolve, projection, transform, itemGraphType, includeNames, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TEntity>> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        IEnumerable<string>? includeNames,
        bool omitQueryArguments)
        where TEntity : class
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var field = new FieldType
        {
            Name = name,
            Type = nonNullType.MakeGenericType(listGraphType.MakeGenericType(itemGraphType))
        };

        if (!omitQueryArguments)
        {
            field.Arguments = ArgumentAppender.GetQueryArguments(hasId, true, false, omitQueryArguments);
        }

        IncludeAppender.SetIncludeMetadata(field, name, includeNames);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                var fieldContext = BuildContext(context);
                var entities = resolve(fieldContext);

                if (entities is IQueryable)
                {
                    throw new("AddProjectedNavigationListField expects IEnumerable, not IQueryable. Use AddProjectedField instead.");
                }

                entities = entities.ApplyGraphQlArguments(hasId, context, omitQueryArguments);

                if (fieldContext.Filters is not null)
                {
                    entities = await fieldContext.Filters.ApplyFilter(entities, context.UserContext, fieldContext.DbContext, context.User);
                }

                var results = new List<TReturn>();
                foreach (var entity in entities)
                {
                    try
                    {
                        var projectedData = compiledProjection(entity);
                        var transformed = await transform(fieldContext, projectedData);
                        results.Add(transformed);
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
                            Failed to project/transform entity in list field `{name}`
                            GraphType: {field.Type.FullName}
                            TSource: {typeof(TSource).FullName}
                            TEntity: {typeof(TEntity).FullName}
                            TProjection: {typeof(TProjection).FullName}
                            TReturn: {typeof(TReturn).FullName}
                            """,
                            exception);
                    }
                }

                return results;
            });

        return field;
    }

    #endregion

    #region AddProjectedField

    // TSource generic variants

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncResolve = (ResolveEfFieldContext<TDbContext, TSource> ctx) => Task.FromResult(resolve(ctx));
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => Task.FromResult(transform(proj));
        var field = BuildProjectedField(name, asyncResolve, projection, asyncTransform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncResolve = (ResolveEfFieldContext<TDbContext, TSource> ctx) => Task.FromResult<IQueryable<TEntity>?>(resolve(ctx));
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => transform(proj);
        var field = BuildProjectedField(name, asyncResolve, projection, asyncTransform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncResolve = (ResolveEfFieldContext<TDbContext, TSource> ctx) => Task.FromResult<IQueryable<TEntity>?>(resolve(ctx));
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> ctx, TProjection proj) => Task.FromResult(transform(ctx, proj));
        var field = BuildProjectedField(name, asyncResolve, projection, asyncTransform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncResolve = (ResolveEfFieldContext<TDbContext, TSource> ctx) => Task.FromResult<IQueryable<TEntity>?>(resolve(ctx));
        var field = BuildProjectedField(name, asyncResolve, projection, transform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => Task.FromResult(transform(proj));
        var field = BuildProjectedField(name, resolve, projection, asyncTransform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> _, TProjection proj) => transform(proj);
        var field = BuildProjectedField(name, resolve, projection, asyncTransform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var asyncTransform = (ResolveEfFieldContext<TDbContext, TSource> ctx, TProjection proj) => Task.FromResult(transform(ctx, proj));
        var field = BuildProjectedField(name, resolve, projection, asyncTransform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedField(name, resolve, projection, transform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    // object source variants

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    public FieldBuilder<object, TReturn> AddProjectedField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TEntity : class
        where TReturn : class =>
        AddProjectedField<object, TEntity, TProjection, TReturn>(graph, name, resolve, projection, transform, graphType);

    FieldType BuildProjectedField<TSource, TEntity, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType)
        where TEntity : class
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var compiledProjection = projection.Compile();

        var fieldType = new FieldType
        {
            Name = name,
            Type = graphType,
            Resolver = new FuncFieldResolver<TSource, TReturn>(
                async context =>
                {
                    var fieldContext = BuildContext(context);

                    var task = resolve(fieldContext);
                    if (task is null)
                    {
                        return default;
                    }

                    var query = await task;
                    if (query is null)
                    {
                        return default;
                    }

                    try
                    {
                        if (disableTracking)
                        {
                            query = query.AsNoTracking();
                        }

                        query = includeAppender.AddIncludes(query, context);

                        var names = GetKeyNames<TEntity>();
                        query = query.ApplyGraphQlArguments(context, names, false, false);

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
                            return default;
                        }

                        if (fieldContext.Filters is not null &&
                            !await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, entity))
                        {
                            return default;
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
                             Failed to execute projected field `{name}`
                             GraphType: {graphType.FullName}
                             TSource: {typeof(TSource).FullName}
                             TEntity: {typeof(TEntity).FullName}
                             TProjection: {typeof(TProjection).FullName}
                             TReturn: {typeof(TReturn).FullName}
                             Query: {query.SafeToQueryString()}
                             """,
                            exception);
                    }
                })
        };

        return fieldType;
    }

    #endregion
}
