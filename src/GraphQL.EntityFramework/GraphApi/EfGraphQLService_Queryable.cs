using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldType AddQueryField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        string? description = null)
        where TReturn : class
    {
        var field = BuildQueryField(graphType, name, resolve, arguments, description);
        return graph.AddField(field);
    }

    public FieldType AddQueryField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        string? description = null)
        where TReturn : class
    {
        var field = BuildQueryField(itemGraphType, name, resolve, arguments, description);
        return graph.AddField(field);
    }

    FieldType BuildQueryField<TSource, TReturn>(
        Type? itemGraphType,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve,
        IEnumerable<QueryArgument>? arguments,
        string? description)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        var fieldType = new FieldType
        {
            Name = name,
            Description = description,
            Type = MakeListGraphType<TReturn>(itemGraphType),
            Arguments = ArgumentAppender.GetQueryArguments(arguments, hasId, true),
        };

        if (resolve is not null)
        {
            fieldType.Resolver = new FuncFieldResolver<TSource, IEnumerable<TReturn>>(
                async context =>
                {
                    var fieldContext = BuildContext(context);
                    var names = GetKeyNames<TReturn>();
                    var query = resolve(fieldContext);
                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    query = query.ApplyGraphQlArguments(context, names, true);

                    QueryLogger.Write(query);

                    List<TReturn> list;
                    if (disableAsync)
                    {
                        list = query.ToList();
                    }
                    else
                    {
                        list = await query
                            .ToListAsync(context.CancellationToken);
                    }

                    return await fieldContext.Filters.ApplyFilter(list, context.UserContext);
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