namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addEnumerableConnectionWithProjection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddEnumerableConnectionWithProjection", BindingFlags.Instance | BindingFlags.NonPublic)!;

    static MethodInfo addEnumerableConnectionWithProjectionOnly = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddEnumerableConnectionWithProjectionOnly", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addEnumerableConnectionWithProjection.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn), typeof(TProjection));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
                projection,
                resolve,
                omitQueryArguments
            };
            return (ConnectionBuilder<TSource>) addConnectionT.Invoke(this, arguments)!;
        }
        catch (Exception exception)
        {
            throw new(
                $"""
                 Failed to execute navigation connection for field `{name}`
                 ItemGraphType: {itemGraphType.FullName}
                 TSource: {typeof(TSource).FullName}
                 TReturn: {typeof(TReturn).FullName}
                 """,
                exception);
        }
    }

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection,
        Type? itemGraphType = null)
        where TReturn : class
    {
        Ensure.NotWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addEnumerableConnectionWithProjectionOnly.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
                projection
            };
            return (ConnectionBuilder<TSource>) addConnectionT.Invoke(this, arguments)!;
        }
        catch (Exception exception)
        {
            throw new(
                $"""
                 Failed to execute navigation connection for field `{name}`
                 ItemGraphType: {itemGraphType.FullName}
                 TSource: {typeof(TSource).FullName}
                 TReturn: {typeof(TReturn).FullName}
                 """,
                exception);
        }
    }

    // Use via reflection
    // ReSharper disable once UnusedMember.Local
    ConnectionBuilder<TSource> AddEnumerableConnection<TSource, TGraph, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve,
        IEnumerable<string>? includeNames,
        bool omitQueryArguments)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        IncludeAppender.SetIncludeMetadata(builder.FieldType, name, includeNames);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        if (resolve is not null)
        {
            builder.ResolveAsync(async context =>
            {
                var efFieldContext = BuildContext(context);

                IEnumerable<TReturn> enumerable;
                try
                {
                    enumerable = resolve(efFieldContext);
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
                         TGraph: {typeof(TGraph).FullName}
                         TSource: {typeof(TSource).FullName}
                         TReturn: {typeof(TReturn).FullName}
                         """,
                        exception);
                }

                if (enumerable is IQueryable)
                {
                    throw new("This API expects the resolver to return a IEnumerable, not an IQueryable. Instead use AddQueryConnectionField.");
                }

                enumerable = enumerable.ApplyGraphQlArguments(hasId, context, omitQueryArguments);
                if (efFieldContext.Filters != null)
                {
                    enumerable = await efFieldContext.Filters.ApplyFilter(enumerable, context.UserContext, efFieldContext.DbContext, context.User);
                }

                var page = enumerable.ToList();

                return ConnectionConverter.ApplyConnectionContext(
                    page,
                    context.First,
                    context.After,
                    context.Last,
                    context.Before);
            });
        }

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = typeof(NonNullGraphType<ConnectionType<TGraph, EdgeType<TGraph>>>);
        var field = graph.AddField(builder.FieldType);

        field.AddWhereArgument(hasId);
        return builder;
    }

    // Use via reflection
    // ReSharper disable once UnusedMember.Local
    ConnectionBuilder<TSource> AddEnumerableConnectionWithProjection<TSource, TGraph, TReturn, TProjection>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, TProjection>> projection,
        Func<ResolveProjectionContext<TDbContext, TProjection>, IEnumerable<TReturn>> resolve,
        bool omitQueryArguments)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        IncludeAppender.SetProjectionMetadata(builder.FieldType, projection, typeof(TSource));

        var compiledProjection = projection.Compile();

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        builder.ResolveAsync(async context =>
        {
            var efFieldContext = BuildContext(context);
            var projected = compiledProjection(context.Source);

            var projectionContext = new ResolveProjectionContext<TDbContext, TProjection>
            {
                Projection = projected,
                DbContext = efFieldContext.DbContext,
                User = context.User,
                Filters = efFieldContext.Filters,
                FieldContext = context
            };

            IEnumerable<TReturn> enumerable;
            try
            {
                enumerable = resolve(projectionContext);
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
                     TGraph: {typeof(TGraph).FullName}
                     TSource: {typeof(TSource).FullName}
                     TReturn: {typeof(TReturn).FullName}
                     """,
                    exception);
            }

            if (enumerable is IQueryable)
            {
                throw new("This API expects the resolver to return a IEnumerable, not an IQueryable. Instead use AddQueryConnectionField.");
            }

            enumerable = enumerable.ApplyGraphQlArguments(hasId, context, omitQueryArguments);
            if (efFieldContext.Filters != null)
            {
                enumerable = await efFieldContext.Filters.ApplyFilter(enumerable, context.UserContext, efFieldContext.DbContext, context.User);
            }

            var page = enumerable.ToList();

            return ConnectionConverter.ApplyConnectionContext(
                page,
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

    // Use via reflection
    // ReSharper disable once UnusedMember.Local
    ConnectionBuilder<TSource> AddEnumerableConnectionWithProjectionOnly<TSource, TGraph, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Expression<Func<TSource, IEnumerable<TReturn>?>> projection)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        IncludeAppender.SetProjectionMetadata(builder.FieldType, projection, typeof(TSource));

        // No resolver set - this overload is for interface types where the concrete types provide resolvers
        // The projection metadata flows through to the Select expression builder

        var hasId = keyNames.ContainsKey(typeof(TReturn));

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = typeof(NonNullGraphType<ConnectionType<TGraph, EdgeType<TGraph>>>);
        var field = graph.AddField(builder.FieldType);

        field.AddWhereArgument(hasId);
        return builder;
    }
}
