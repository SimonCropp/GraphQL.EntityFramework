namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IOrderedQueryable<TReturn>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        BuildQueryConnection<object, TReturn>(
            graph, name,
            resolve == null ? null : context => Task.FromResult(resolve(context)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, Task<IOrderedQueryable<TReturn>?>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        BuildQueryConnection(graph, name, resolve, itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IOrderedQueryable<TReturn>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        BuildQueryConnection<TSource, TReturn>(
            graph, name,
            resolve == null ? null : context => Task.FromResult(resolve(context)),
            itemGraphType, omitQueryArguments);

    public ConnectionBuilder<TSource> AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TReturn>?>?>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false)
        where TReturn : class =>
        BuildQueryConnection(graph, name, resolve, itemGraphType, omitQueryArguments);

    ConnectionBuilder<TSource> BuildQueryConnection<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IOrderedQueryable<TReturn>?>?>? resolve,
        Type? itemGraphType,
        bool omitQueryArguments)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var builder = ConnectionBuilderEx<TSource>.Build(name, itemGraphType);

        if (resolve is not null)
        {
            var names = GetKeyNames<TReturn>();
            builder.ResolveAsync(async context =>
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

                query = query.ApplyGraphQlArguments(context, names, true, omitQueryArguments);

                if (includeAppender.TryGetProjectionExpressionWithFilters<TDbContext, TReturn>(context, fieldContext.Filters, out var selectExpr))
                {
                    query = query.Select(selectExpr);
                }
                else
                {
                    query = includeAppender.AddIncludes(context, fieldContext.Filters, query);
                }

                try
                {
                    return await query.ApplyConnectionContext(
                        context.First,
                        context.After!,
                        context.Last,
                        context.Before!,
                        context,
                        context.CancellationToken,
                        fieldContext.Filters,
                        fieldContext.DbContext);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    throw new(
                        $"""
                         Failed to execute query for field `{name}`
                         ItemGraphType: {itemGraphType.FullName}
                         TSource: {typeof(TSource).FullName}
                         TReturn: {typeof(TReturn).FullName}
                         KeyNames: {JoinKeys(names)}
                         Query: {query.SafeToQueryString()}
                         """,
                        exception);
                }
            });
        }

        //TODO: works around https://github.com/graphql-dotnet/graphql-dotnet/pull/2581/
        builder.FieldType.Type = ConnectionBuilderEx<TSource>.NonNullConnectionType(itemGraphType);
        var field = graph.AddField(builder.FieldType);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        field.AddWhereArgument(hasId);
        return builder;
    }
}
