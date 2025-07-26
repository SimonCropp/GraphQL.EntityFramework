namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addEnumerableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddEnumerableConnection", BindingFlags.Instance | BindingFlags.NonPublic)!;

    public ConnectionBuilder<TSource> AddNavigationConnectionField<TSource, TReturn>(
        ComplexGraphType<TSource> graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<string>? includeNames = null,
        bool omitQueryArguments = false)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        var addConnectionT = addEnumerableConnection.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn));

        try
        {
            var arguments = new object?[]
            {
                graph,
                name,
                resolve,
                includeNames,
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
}