namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addQueryableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddQueryableConnection", BindingFlags.Instance| BindingFlags.NonPublic)!;

    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TReturn>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddQueryConnectionField<TReturn>(
            graph,
            name,
            resolve == null ? null : context => Task.FromResult(resolve(context)),
            itemGraphType,
            omitQueryArguments);

    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TReturn>?>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var addConnectionT = addQueryableConnection.MakeGenericMethod(typeof(object), itemGraphType, typeof(TReturn));
        try
        {
            return (ConnectionBuilder<object>) addConnectionT.Invoke(
                this,
                [
                    graph,
                    name,
                    resolve,
                    omitQueryArguments
                ])!;
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
                ItemGraphType: {itemGraphType.FullName}
                TReturn: {typeof(TReturn).FullName}
                """,
                exception);
        }
    }

    public ConnectionBuilder<TSource> AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TReturn>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        AddQueryConnectionField<TSource, TReturn>(
            graph,
            name,
            resolve == null ? null : context => Task.FromResult(resolve(context)),
            itemGraphType,
            omitQueryArguments);

    public ConnectionBuilder<TSource> AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TReturn>?>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var addConnectionT = addQueryableConnection.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn));

        try
        {
            return (ConnectionBuilder<TSource>) addConnectionT.Invoke(
                this,
                [
                    graph,
                    name,
                    resolve,
                    omitQueryArguments
                ])!;
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
                ItemGraphType: {itemGraphType.FullName}
                TSource: {typeof(TSource).FullName}
                TReturn: {typeof(TReturn).FullName}
                """,
                exception);
        }
    }

    ConnectionBuilder<TSource> AddQueryableConnection<TSource, TGraph, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TReturn>?>?>? resolve,
        bool omitQueryArguments)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        if (resolve is not null)
        {
            var names = GetKeyNames<TReturn>();
            builder.ResolveAsync(
                async context =>
                {
                    var fieldContext = BuildContext(context);

                    var task = resolve(fieldContext);
                    if (task == null)
                    {
                        return new Connection<TSource>();
                    }

                    IQueryable<TReturn>? query = await task;
                    if (query == null)
                    {
                        return new Connection<TSource>();
                    }

                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    query = query.ApplyGraphQlArguments(context, names, true, omitQueryArguments);

                    try
                    {
                        return await query
                            .ApplyConnectionContext(
                                context.First,
                                context.After!,
                                context.Last,
                                context.Before!,
                                context,
                                context.CancellationToken,
                                fieldContext.Filters,
                                fieldContext.DbContext);
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
                             KeyNames: {JoinKeys(names)}
                             Query: {query.ToQueryString()}
                             """,
                            exception);
                    }
                });
        }

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = typeof(NonNullGraphType<ConnectionType<TGraph, EdgeType<TGraph>>>);
        var field = graph.AddField(builder.FieldType);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        field.AddWhereArgument(hasId);
        return builder;
    }
}