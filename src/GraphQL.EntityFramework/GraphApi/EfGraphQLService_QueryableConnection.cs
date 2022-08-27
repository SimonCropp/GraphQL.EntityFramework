namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addQueryableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddQueryableConnection", BindingFlags.Instance| BindingFlags.NonPublic)!;

    public ConnectionBuilder<object> AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var addConnectionT = addQueryableConnection.MakeGenericMethod(typeof(object), itemGraphType, typeof(TReturn));
        return (ConnectionBuilder<object>)addConnectionT.Invoke(this, new object?[] { graph, name, resolve})!;
    }

    public ConnectionBuilder<TSource> AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var addConnectionT = addQueryableConnection.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn));
        return (ConnectionBuilder<TSource>) addConnectionT.Invoke(this, new object?[] { graph, name, resolve })!;
    }

    ConnectionBuilder<TSource> AddQueryableConnection<TSource, TGraph, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilderEx<TSource>.Build<TGraph>(name);

        if (resolve is not null)
        {
            builder.ResolveAsync(
                async context =>
                {
                    var efFieldContext = BuildContext(context);
                    var query = resolve(efFieldContext);
                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    var names = GetKeyNames<TReturn>();
                    query = query.ApplyGraphQlArguments(context, names, true);
                    return await query
                        .ApplyConnectionContext(
                            context.First,
                            context.After!,
                            context.Last,
                            context.Before!,
                            context,
                            context.CancellationToken,
                            efFieldContext.Filters);
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