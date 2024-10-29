namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldBuilder<object, TReturn> AddQueryField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? graphType = null,
        bool omitQueryArguments = false,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>, IOrderedQueryable<TReturn>>? orderBy = null)
        where TReturn : class
    {
        var field = BuildQueryField(graphType, name, resolve, omitQueryArguments, orderBy);
        graph.AddField(field);
        return new FieldBuilderEx<object, TReturn>(field);
    }

    public FieldBuilder<TSource, TReturn> AddQueryField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        bool omitQueryArguments = false,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>, IOrderedQueryable<TReturn>>? orderBy = null)
        where TReturn : class
    {
        var field = BuildQueryField(itemGraphType, name, resolve, omitQueryArguments, orderBy);
        graph.AddField(field);
        return new FieldBuilderEx<TSource, TReturn>(field);
    }

    FieldType BuildQueryField<TSource, TReturn>(
        Type? itemGraphType,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve,
        bool omitQueryArguments,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>, IOrderedQueryable<TReturn>>? orderBy)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var fieldType = new FieldType
        {
            Name = name,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(hasId, true, false, omitQueryArguments),
        };

        var names = GetKeyNames<TReturn>();
        if (resolve is not null)
        {
            fieldType.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
                async context =>
                {
                    var fieldContext = BuildContext(context);
                    var query = resolve(fieldContext);
                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    if (!omitQueryArguments)
                    {
                        query = query.ApplyGraphQlArguments(context, names, true, omitQueryArguments);
                    }

                    if(orderBy is not null)
                    {
                        query = orderBy(fieldContext, query);
                    }

                    QueryLogger.Write(query);

                    List<TReturn> list;

                    try
                    {
                        if (disableAsync)
                        {
                            list = query.ToList();
                        }
                        else
                        {
                            list = await query
                                .ToListAsync(context.CancellationToken);
                        }
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
                             GraphType: {fieldType.Type.FullName}
                             TSource: {typeof(TSource).FullName}
                             TReturn: {typeof(TReturn).FullName}
                             DisableTracking: {disableTracking}
                             HasId: {hasId}
                             DisableAsync: {disableAsync}
                             KeyNames: {JoinKeys(names)}
                             Query: {query.ToQueryString()}
                             """,
                            exception);
                    }

                    return await fieldContext.Filters.ApplyFilter(list, context.UserContext, context.User);
                });
        }

        return fieldType;
    }

    static Type listGraphType = typeof(ListGraphType<>);
    static Type nonNullType = typeof(NonNullGraphType<>);

    static Type MakeListGraphType<TReturn>(Type? itemGraphType)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

        return nonNullType.MakeGenericType(listGraphType.MakeGenericType(itemGraphType));
    }

    List<string>? GetKeyNames<TSource>()
    {
        keyNames.TryGetValue(typeof(TSource), out var names);
        return names;
    }
}