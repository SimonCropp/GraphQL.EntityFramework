namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    #region AddProjectedNavigationField

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationField(name, projection, (_, proj) => Task.FromResult(transform(proj)), graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationField(name, projection, (_, proj) => transform(proj), graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationField(name, projection, (ctx, proj) => Task.FromResult(transform(ctx, proj)), graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationField(name, projection, transform, graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildProjectedNavigationField<TSource, TProjection, TReturn>(
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType)
        where TSource : class
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

        // Extract navigation includes from projection
        var autoIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);
        IncludeAppender.SetIncludeMetadata(field, name, autoIncludes);

        field.Resolver = new FuncFieldResolver<TSource, TReturn>(
            async context =>
            {
                var fieldContext = BuildContext(context);

                if (fieldContext.Filters is not null &&
                    !await fieldContext.Filters.ShouldInclude(context.UserContext, fieldContext.DbContext, context.User, context.Source))
                {
                    return default;
                }

                var projectedData = compiledProjection(context.Source);
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
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationListField(name, navigation, projection, (_, proj) => Task.FromResult(transform(proj)), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationListField(name, navigation, projection, (_, proj) => transform(proj), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationListField(name, navigation, projection, (ctx, proj) => Task.FromResult(transform(ctx, proj)), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class
    {
        var field = BuildProjectedNavigationListField(name, navigation, projection, transform, itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildProjectedNavigationListField<TSource, TEntity, TProjection, TReturn>(
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TSource : class
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

        // Extract navigation includes from navigation expression
        var navigationIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(navigation, model);

        // Extract nested navigation includes from projection expression
        var nestedIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);

        // Merge: navigation includes + prefixed nested includes
        var mergedIncludes = MergeIncludes(nestedIncludes, navigationIncludes);
        IncludeAppender.SetIncludeMetadata(field, name, mergedIncludes);

        var compiledNavigation = navigation.Compile();
        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                var fieldContext = BuildContext(context);
                var entities = compiledNavigation(context.Source);

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
        var field = BuildProjectedField<TSource, TEntity, TProjection, TReturn>(
            name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (_, proj) => Task.FromResult(transform(proj)),
            graphType);
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
        var field = BuildProjectedField<TSource, TEntity, TProjection, TReturn>(
            name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (_, projection) => transform(projection),
            graphType);
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
        var field = BuildProjectedField<TSource, TEntity, TProjection, TReturn>(
            name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (context, projection) => Task.FromResult(transform(context, projection)),
            graphType);
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
        var field = BuildProjectedField(
            name,
            context => Task.FromResult(resolve(context)),
            projection,
            transform,
            graphType);
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
        var field = BuildProjectedField(
            name, resolve,
            projection,
            (_, projection) => Task.FromResult(transform(projection)),
            graphType);
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
        var field = BuildProjectedField(
            name,
            resolve,
            projection,
            (_, projection) => transform(projection),
            graphType);
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
        var field = BuildProjectedField(name, resolve, projection, (ctx, proj) => Task.FromResult(transform(ctx, proj)), graphType);
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

        // Extract navigation includes from projection
        var autoIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);

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

        fieldType.Resolver = new FuncFieldResolver<TSource, TReturn>(
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
                });

        return fieldType;
    }

    #endregion

    static IEnumerable<string>? MergeIncludes(IReadOnlySet<string> autoIncludes, IEnumerable<string>? userIncludes)
    {
        if (autoIncludes.Count == 0 && userIncludes == null)
        {
            return null;
        }

        if (autoIncludes.Count == 0)
        {
            return userIncludes;
        }

        if (userIncludes == null)
        {
            // No user includes - just use auto-detected navigations
            return autoIncludes;
        }

        // Get the base navigation path from userIncludes (first entry)
        var baseNavigationPath = userIncludes.FirstOrDefault();

        if (baseNavigationPath == null)
        {
            // Empty user includes - just use auto-detected navigations
            return autoIncludes;
        }

        // Prefix auto-detected navigations with the base navigation path
        var prefixedAutoIncludes = autoIncludes
            .Select(include => $"{baseNavigationPath}.{include}");

        // Combine user includes with prefixed auto includes and deduplicate
        return userIncludes.Concat(prefixedAutoIncludes).Distinct();
    }
}
