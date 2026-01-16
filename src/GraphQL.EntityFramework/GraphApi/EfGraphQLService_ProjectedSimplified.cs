namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addSimplifiedProjectedEnumerableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddSimplifiedProjectedEnumerableConnection", BindingFlags.Instance | BindingFlags.NonPublic)!;

    static MethodInfo addSimplifiedProjectedQueryableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddSimplifiedProjectedQueryableConnection", BindingFlags.Instance | BindingFlags.NonPublic)!;

    #region AddProjectedField (simplified navigation projection)

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<TProjection, TReturn> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedField(name, projection, (_, proj) => Task.FromResult(transform(proj)), graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null)
        where TSource : class
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedField(name, projection, (_, proj) => transform(proj), graphType);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildSimplifiedProjectedField<TSource, TProjection, TReturn>(
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

    #region AddProjectedListField (simplified list projection)

    public FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedListField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedListField(name, projection, (_, proj) => Task.FromResult(transform(proj)), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, IEnumerable<TReturn>>(field);
    }

    public FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedListField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedListField(name, projection, (_, proj) => transform(proj), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, IEnumerable<TReturn>>(field);
    }

    FieldType BuildSimplifiedProjectedListField<TSource, TProjection, TReturn>(
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TSource : class
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

        // Extract navigation includes from projection expression
        var autoIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);
        IncludeAppender.SetIncludeMetadata(field, name, autoIncludes);

        var compiledProjection = projection.Compile();

        field.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                var fieldContext = BuildContext(context);
                var items = compiledProjection(context.Source);

                if (items is null)
                {
                    return [];
                }

                var itemsList = items.ToList();

                var results = new List<TReturn>();
                foreach (var item in itemsList)
                {
                    try
                    {
                        var transformed = await transform(fieldContext, item);
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
                            Failed to transform item in list field `{name}`
                            GraphType: {field.Type.FullName}
                            TSource: {typeof(TSource).FullName}
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

    #region AddProjectedConnectionField (simplified navigation connection)

    public ConnectionBuilder<TSource> AddProjectedConnectionField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class =>
        AddSimplifiedProjectedConnectionFieldCore(
            graph, name, projection,
            (_, proj) => Task.FromResult(transform(proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedConnectionField<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TSource : class
        where TReturn : class =>
        AddSimplifiedProjectedConnectionFieldCore(
            graph, name, projection,
            (_, proj) => transform(proj),
            itemGraphType, omitQueryArguments);

    ConnectionBuilder<TSource> AddSimplifiedProjectedConnectionFieldCore<TSource, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TSource : class
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addSimplifiedProjectedEnumerableConnection.MakeGenericMethod(
            typeof(TSource), itemGraphType, typeof(TProjection), typeof(TReturn));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
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
                 Failed to execute simplified projected connection for field `{name}`
                 ItemGraphType: {itemGraphType.FullName}
                 TSource: {typeof(TSource).FullName}
                 TProjection: {typeof(TProjection).FullName}
                 TReturn: {typeof(TReturn).FullName}
                 """,
                exception);
        }
    }

    ConnectionBuilder<TSource> AddSimplifiedProjectedEnumerableConnection<TSource, TGraph, TProjection, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TProjection>>> projection,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        bool omitQueryArguments)
        where TSource : class
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        // Extract navigation includes from projection expression
        var autoIncludes = ProjectionIncludeAnalyzer.ExtractNavigationIncludes(projection, model);
        IncludeAppender.SetIncludeMetadata(builder.FieldType, name, autoIncludes);

        var compiledProjection = projection.Compile();
        var hasId = keyNames.ContainsKey(typeof(TReturn));

        builder.ResolveAsync(async context =>
        {
            var efFieldContext = BuildContext(context);
            IEnumerable<TProjection> items;

            try
            {
                items = compiledProjection(context.Source);
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
                     Failed to execute projection for simplified connection field `{name}`
                     TGraph: {typeof(TGraph).FullName}
                     TSource: {typeof(TSource).FullName}
                     TProjection: {typeof(TProjection).FullName}
                     TReturn: {typeof(TReturn).FullName}
                     """,
                    exception);
            }

            if (items is null)
            {
                return ConnectionConverter.ApplyConnectionContext(
                    new List<TReturn>(),
                    context.First,
                    context.After,
                    context.Last,
                    context.Before);
            }

            var itemsList = items.ToList();

            var results = new List<TReturn>();
            foreach (var item in itemsList)
            {
                try
                {
                    var transformed = await transform(efFieldContext, item);
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
                         Failed to transform item in simplified connection field `{name}`
                         TGraph: {typeof(TGraph).FullName}
                         TSource: {typeof(TSource).FullName}
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

    #region AddProjectedSingleField (simplified - resolve returns projected IQueryable)

    public FieldBuilder<TSource, TReturn> AddProjectedSingleField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedSingleField<TSource, TProjection, TReturn>(
            name, ctx => Task.FromResult(resolve(ctx)), (_, proj) => Task.FromResult(transform(proj)), graphType, nullable, useSingle: true);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedSingleField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedSingleField<TSource, TProjection, TReturn>(
            name, ctx => Task.FromResult(resolve(ctx)), (_, proj) => transform(proj), graphType, nullable, useSingle: true);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        AddProjectedSingleField<object, TProjection, TReturn>(graph, name, resolve, transform, graphType, nullable);

    public FieldBuilder<object, TReturn> AddProjectedSingleField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        AddProjectedSingleField<object, TProjection, TReturn>(graph, name, resolve, transform, graphType, nullable);

    #endregion

    #region AddProjectedFirstField (simplified - resolve returns projected IQueryable)

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedSingleField<TSource, TProjection, TReturn>(
            name, ctx => Task.FromResult(resolve(ctx)), (_, proj) => Task.FromResult(transform(proj)), graphType, nullable, useSingle: false);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddProjectedFirstField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedSingleField<TSource, TProjection, TReturn>(
            name, ctx => Task.FromResult(resolve(ctx)), (_, proj) => transform(proj), graphType, nullable, useSingle: false);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        AddProjectedFirstField<object, TProjection, TReturn>(graph, name, resolve, transform, graphType, nullable);

    public FieldBuilder<object, TReturn> AddProjectedFirstField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? graphType = null,
        bool nullable = false)
        where TReturn : class =>
        AddProjectedFirstField<object, TProjection, TReturn>(graph, name, resolve, transform, graphType, nullable);

    #endregion

    #region BuildSimplifiedProjectedSingleField (shared by Single and First)

    FieldType BuildSimplifiedProjectedSingleField<TSource, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TProjection>>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? graphType,
        bool nullable,
        bool useSingle)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>(nullable);

        var fieldType = new FieldType
        {
            Name = name,
            Type = graphType,
            Resolver = new FuncFieldResolver<TSource, TReturn>(
            async context =>
            {
                var fieldContext = BuildContext(context);

                var query = await resolve(fieldContext);
                if (query is null)
                {
                    return default;
                }

                try
                {
                    QueryLogger.Write(query);

                    TProjection? projected;
                    if (query.Provider is IAsyncQueryProvider)
                    {
                        projected = useSingle
                            ? await query.SingleOrDefaultAsync(context.CancellationToken)
                            : await query.FirstOrDefaultAsync(context.CancellationToken);
                    }
                    else
                    {
                        projected = useSingle
                            ? query.SingleOrDefault()
                            : query.FirstOrDefault();
                    }

                    if (projected is null)
                    {
                        return default;
                    }

                    var result = await transform(fieldContext, projected);
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
                        Failed to execute simplified projected {(useSingle ? "single" : "first")} field `{name}`
                        GraphType: {graphType.FullName}
                        TSource: {typeof(TSource).FullName}
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

    #region AddProjectedQueryField (simplified - resolve returns projected IQueryable)

    public FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedQueryField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedQueryField<TSource, TProjection, TReturn>(
            name, ctx => Task.FromResult(resolve(ctx)), (_, proj) => Task.FromResult(transform(proj)), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, IEnumerable<TReturn>>(field);
    }

    public FieldBuilder<TSource, IEnumerable<TReturn>> AddProjectedQueryField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        var field = BuildSimplifiedProjectedQueryField<TSource, TProjection, TReturn>(
            name, ctx => Task.FromResult(resolve(ctx)), (_, proj) => transform(proj), itemGraphType, omitQueryArguments);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, IEnumerable<TReturn>>(field);
    }

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddProjectedQueryField<object, TProjection, TReturn>(graph, name, resolve, transform, itemGraphType, omitQueryArguments);

    public FieldBuilder<object, IEnumerable<TReturn>> AddProjectedQueryField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddProjectedQueryField<object, TProjection, TReturn>(graph, name, resolve, transform, itemGraphType, omitQueryArguments);

    FieldType BuildSimplifiedProjectedQueryField<TSource, TProjection, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TProjection>>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var fieldType = new FieldType
        {
            Name = name,
            Type = nonNullType.MakeGenericType(listGraphType.MakeGenericType(itemGraphType))
        };

        if (!omitQueryArguments)
        {
            fieldType.Arguments = ArgumentAppender.GetQueryArguments(false, true, false, omitQueryArguments);
        }

        fieldType.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
            async context =>
            {
                var fieldContext = BuildContext(context);

                var query = await resolve(fieldContext);
                if (query is null)
                {
                    return [];
                }

                try
                {
                    QueryLogger.Write(query);

                    List<TProjection> projected;
                    if (query.Provider is IAsyncQueryProvider)
                    {
                        projected = await query.ToListAsync(context.CancellationToken);
                    }
                    else
                    {
                        projected = query.ToList();
                    }

                    var results = new List<TReturn>();
                    foreach (var item in projected)
                    {
                        try
                        {
                            var transformed = await transform(fieldContext, item);
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
                                Failed to transform item in simplified query field `{name}`
                                ItemGraphType: {itemGraphType.FullName}
                                TSource: {typeof(TSource).FullName}
                                TProjection: {typeof(TProjection).FullName}
                                TReturn: {typeof(TReturn).FullName}
                                """,
                                exception);
                        }
                    }

                    return results;
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
                        Failed to execute simplified projected query field `{name}`
                        ItemGraphType: {itemGraphType.FullName}
                        TSource: {typeof(TSource).FullName}
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

    #region AddProjectedQueryConnectionField (simplified - resolve returns projected IOrderedQueryable)

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddSimplifiedProjectedQueryConnectionFieldCore<TSource, TProjection, TReturn>(
            graph, name,
            ctx => Task.FromResult(resolve(ctx)),
            (_, proj) => Task.FromResult(transform(proj)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddProjectedQueryConnectionField<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddSimplifiedProjectedQueryConnectionFieldCore<TSource, TProjection, TReturn>(
            graph, name,
            ctx => Task.FromResult(resolve(ctx)),
            (_, proj) => transform(proj),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, TReturn> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TProjection, TReturn>(graph, name, resolve, transform, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddProjectedQueryConnectionField<TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TProjection>> resolve,
        Func<TProjection, Task<TReturn>> transform,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddProjectedQueryConnectionField<object, TProjection, TReturn>(graph, name, resolve, transform, itemGraphType, omitQueryArguments);

    ConnectionBuilder<TSource> AddSimplifiedProjectedQueryConnectionFieldCore<TSource, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TProjection>>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addSimplifiedProjectedQueryableConnection.MakeGenericMethod(
            typeof(TSource), itemGraphType, typeof(TProjection), typeof(TReturn));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
                resolve,
                transform,
                omitQueryArguments
            };
            return (ConnectionBuilder<TSource>)addConnectionT.Invoke(this, arguments)!;
        }
        catch (Exception exception)
        {
            throw new(
                $"""
                 Failed to execute simplified projected query connection for field `{name}`
                 ItemGraphType: {itemGraphType.FullName}
                 TSource: {typeof(TSource).FullName}
                 TProjection: {typeof(TProjection).FullName}
                 TReturn: {typeof(TReturn).FullName}
                 """,
                exception);
        }
    }

    ConnectionBuilder<TSource> AddSimplifiedProjectedQueryableConnection<TSource, TGraph, TProjection, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TProjection>>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TProjection, Task<TReturn>> transform,
        bool omitQueryArguments)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);
        var hasId = keyNames.ContainsKey(typeof(TReturn));

        builder.ResolveAsync(async context =>
        {
            var fieldContext = BuildContext(context);

            var query = await resolve(fieldContext);
            if (query is null)
            {
                return new Connection<TSource>();
            }

            try
            {
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

                var projected = await page.ToListAsync(context.CancellationToken);
                context.CancellationToken.ThrowIfCancellationRequested();

                // Transform each item
                var results = new List<TReturn>();
                foreach (var item in projected)
                {
                    try
                    {
                        var transformed = await transform(fieldContext, item);
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
                             Failed to transform item in simplified query connection field `{name}`
                             TGraph: {typeof(TGraph).FullName}
                             TSource: {typeof(TSource).FullName}
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
                     Failed to execute simplified projected query connection for field `{name}`
                     TGraph: {typeof(TGraph).FullName}
                     TSource: {typeof(TSource).FullName}
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
