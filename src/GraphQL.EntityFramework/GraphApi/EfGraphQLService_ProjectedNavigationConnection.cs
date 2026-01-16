namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addProjectedEnumerableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddProjectedEnumerableConnection", BindingFlags.Instance | BindingFlags.NonPublic)!;

    #region AddProjectedNavigationConnectionField

    public ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class =>
        AddProjectedNavigationConnectionFieldCore(
            graph, name, navigation, projection,
            (_, proj) => Task.FromResult(transform(proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class =>
        AddProjectedNavigationConnectionFieldCore(
            graph, name, navigation, projection,
            (_, proj) => transform(proj),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class =>
        AddProjectedNavigationConnectionFieldCore(
            graph, name, navigation, projection,
            (ctx, proj) => Task.FromResult(transform(ctx, proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedNavigationConnectionField<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TEntity : class
        where TReturn : class =>
        AddProjectedNavigationConnectionFieldCore(
            graph, name, navigation, projection,
            transform,
            itemGraphType, omitQueryArguments);

    ConnectionBuilder<TSource> AddProjectedNavigationConnectionFieldCore<TSource, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
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

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addProjectedEnumerableConnection.MakeGenericMethod(
            typeof(TSource), itemGraphType, typeof(TEntity), typeof(TProjection), typeof(TReturn));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
                navigation,
                projection,
                transform,
                omitQueryArguments
            };
            return (ConnectionBuilder<TSource>)addConnectionT.Invoke(this, arguments)!;
        }
        catch (Exception exception)
        {
            throw new(
                $"""
                 Failed to execute projected navigation connection for field `{name}`
                 ItemGraphType: {itemGraphType.FullName}
                 TSource: {typeof(TSource).FullName}
                 TEntity: {typeof(TEntity).FullName}
                 TProjection: {typeof(TProjection).FullName}
                 TReturn: {typeof(TReturn).FullName}
                 """,
                exception);
        }
    }

    ConnectionBuilder<TSource> AddProjectedEnumerableConnection<TSource, TGraph, TEntity, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TEntity>>> navigation,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        bool omitQueryArguments)
        where TSource : class
        where TGraph : IGraphType
        where TEntity : class
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        // Extract navigation includes from navigation expression
        var navigationIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(navigation, model);

        // Extract nested navigation includes from projection expression
        var nestedIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);

        // Merge: navigation includes + prefixed nested includes
        var mergedIncludes = MergeIncludes(nestedIncludes, navigationIncludes);
        IncludeAppender.SetIncludeMetadata(builder.FieldType, name, mergedIncludes);

        var compiledNavigation = navigation.Compile();
        var compiledProjection = projection.Compile();

        var hasId = keyNames.ContainsKey(typeof(TReturn));

        builder.ResolveAsync(async context =>
        {
            var efFieldContext = BuildContext(context);
            IEnumerable<TEntity> entities;

            try
            {
                entities = compiledNavigation(context.Source);
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
                     Failed to execute navigation for projected connection field `{name}`
                     TGraph: {typeof(TGraph).FullName}
                     TSource: {typeof(TSource).FullName}
                     TEntity: {typeof(TEntity).FullName}
                     TProjection: {typeof(TProjection).FullName}
                     TReturn: {typeof(TReturn).FullName}
                     """,
                    exception);
            }

            if (entities is IQueryable)
            {
                throw new("AddProjectedNavigationConnectionField expects IEnumerable, not IQueryable. Use AddProjectedQueryConnectionField instead.");
            }

            entities = entities.ApplyGraphQlArguments(hasId, context, omitQueryArguments);

            if (efFieldContext.Filters is not null)
            {
                entities = await efFieldContext.Filters.ApplyFilter(entities, context.UserContext, efFieldContext.DbContext, context.User);
            }

            var results = new List<TReturn>();
            foreach (var entity in entities)
            {
                try
                {
                    var projectedData = compiledProjection(entity);
                    var transformed = await transform(efFieldContext, projectedData);
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
                         Failed to project/transform entity in connection field `{name}`
                         TGraph: {typeof(TGraph).FullName}
                         TSource: {typeof(TSource).FullName}
                         TEntity: {typeof(TEntity).FullName}
                         TProjection: {typeof(TProjection).FullName}
                         TReturn: {typeof(TReturn).FullName}
                         """,
                        exception);
                }
            }

            return ConnectionConverter.ApplyConnectionContext(
                results,
                context.First,
                context.After,
                context.Last,
                context.Before);
        });

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = typeof(NonNullGraphType<ConnectionType<TGraph, EdgeType<TGraph>>>);
        var field = graph.AddField(builder.FieldType);

        field.AddWhereArgument(hasId);
        return builder;
    }

    #endregion
}
