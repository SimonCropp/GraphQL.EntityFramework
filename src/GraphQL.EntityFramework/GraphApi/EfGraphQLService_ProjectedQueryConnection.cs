namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addProjectedQueryableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddProjectedQueryableConnection", BindingFlags.Instance | BindingFlags.NonPublic)!;

    #region AddProjectedQueryConnectionField - TSource generic variants

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore<TSource, TEntity, TProjection, TReturn>(
            graph, name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (_, proj) => Task.FromResult(transform(proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore<TSource, TEntity, TProjection, TReturn>(
            graph, name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (_, proj) => transform(proj),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore<TSource, TEntity, TProjection, TReturn>(
            graph, name,
            _ => Task.FromResult(resolve(_)),
            projection,
            (ctx, proj) => Task.FromResult(transform(ctx, proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore<TSource, TEntity, TProjection, TReturn>(
            graph, name,
            context => Task.FromResult(resolve(context)),
            projection,
            transform,
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore(
            graph, name,
            resolve,
            projection,
            (_, proj) => Task.FromResult(transform(proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore(
            graph, name,
            resolve,
            projection,
            (_, proj) => transform(proj),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore(
            graph, name,
            resolve,
            projection,
            (ctx, proj) => Task.FromResult(transform(ctx, proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionFieldCore(
            graph, name,
            resolve,
            projection,
            transform,
            itemGraphType, omitQueryArguments);

    ConnectionBuilder<TSource> AddProjectedQueryConnectionFieldCore<TSource, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TEntity : class
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addProjectedQueryableConnection.MakeGenericMethod(
            typeof(TSource), itemGraphType, typeof(TEntity), typeof(TProjection), typeof(TReturn));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
                resolve,
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
                 Failed to execute projected query connection for field `{name}`
                 ItemGraphType: {itemGraphType.FullName}
                 TSource: {typeof(TSource).FullName}
                 TEntity: {typeof(TEntity).FullName}
                 TProjection: {typeof(TProjection).FullName}
                 TReturn: {typeof(TReturn).FullName}
                 """,
                exception);
        }
    }

    #endregion

    #region AddProjectedQueryConnectionField - object source variants

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TEntity>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, object>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TEntity : class
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TEntity, TProjection, TReturn>(
            graph, name, resolve, projection, transform, itemGraphType, omitQueryArguments);

    #endregion

    #region AddProjectedQueryableConnection

    ConnectionBuilder<TSource> AddProjectedQueryableConnection<TSource, TGraph, TEntity, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TEntity>?>?> resolve,
        Expression<Func<TEntity, TProjection>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        bool omitQueryArguments)
        where TGraph : IGraphType
        where TEntity : class
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        // Extract navigation includes from projection
        var autoIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);
        IncludeAppender.SetIncludeMetadata(builder.FieldType, name, autoIncludes);

        var compiledProjection = projection.Compile();
        var names = GetKeyNames<TEntity>();
        var hasId = keyNames.ContainsKey(typeof(TEntity));

        builder.ResolveAsync(async context =>
        {
            var fieldContext = BuildContext(context);

            var task = resolve(fieldContext);
            if (task is null)
            {
                return new Connection<TSource>();
            }

            IQueryable<TEntity>? query = await task;
            if (query is null)
            {
                return new Connection<TSource>();
            }

            try
            {
                if (disableTracking)
                {
                    query = query.AsNoTracking();
                }

                query = includeAppender.AddIncludes(query, context);
                query = query.ApplyGraphQlArguments(context, names, true, omitQueryArguments);

                // Parse pagination parameters
                int? after = null;
                if (context.After is not null)
                {
                    after = int.Parse(context.After);
                }

                int? before = null;
                if (context.Before is not null)
                {
                    before = int.Parse(context.Before);
                }

                // Get total count
                var count = await query.CountAsync(context.CancellationToken);
                context.CancellationToken.ThrowIfCancellationRequested();

                // Calculate skip and take
                int skip;
                int take;

                if (context.Last is null)
                {
                    // First/After pagination
                    take = context.First.GetValueOrDefault(0);
                    if (before is null)
                    {
                        skip = after + 1 ?? 0;
                    }
                    else
                    {
                        skip = Math.Max(before.Value - take, 0);
                    }
                }
                else
                {
                    // Last/Before pagination
                    take = context.Last.Value;
                    if (after is null)
                    {
                        skip = before.GetValueOrDefault(count) - take;
                    }
                    else
                    {
                        skip = after.Value + 1;
                    }
                }

                // Execute paginated query
                var page = query.Skip(skip).Take(take);
                QueryLogger.Write(page);

                IEnumerable<TEntity> entities = await page.ToListAsync(context.CancellationToken);
                context.CancellationToken.ThrowIfCancellationRequested();

                // Apply filters
                if (fieldContext.Filters is not null)
                {
                    entities = await fieldContext.Filters.ApplyFilter(
                        entities, context.UserContext, fieldContext.DbContext, context.User);
                }

                // Project and transform each entity
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

                // Build connection with transformed results
                var edges = results
                    .Select((item, index) =>
                        new Edge<TReturn>
                        {
                            Cursor = (index + skip).ToString(),
                            Node = item
                        })
                    .ToList();

                return new Connection<TReturn>
                {
                    TotalCount = count,
                    Edges = edges,
                    PageInfo = new()
                    {
                        HasNextPage = count > take + skip,
                        HasPreviousPage = skip > 0 && take < count,
                        StartCursor = skip.ToString(),
                        EndCursor = Math.Min(count - 1, take - 1 + skip).ToString()
                    }
                };
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
                     Failed to execute projected query connection for field `{name}`
                     TGraph: {typeof(TGraph).FullName}
                     TSource: {typeof(TSource).FullName}
                     TEntity: {typeof(TEntity).FullName}
                     TProjection: {typeof(TProjection).FullName}
                     TReturn: {typeof(TReturn).FullName}
                     Query: {query.SafeToQueryString()}
                     """,
                    exception);
            }
        });

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = typeof(NonNullGraphType<ConnectionType<TGraph, EdgeType<TGraph>>>);
        var field = graph.AddField(builder.FieldType);

        field.AddWhereArgument(hasId);
        return builder;
    }

    #endregion
}
