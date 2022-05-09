using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    public FieldType AddSingleField<TReturn>(
        IObjectGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class
    {
        var field = BuildSingleField(name, resolve, mutate, arguments, graphType, nullable, description);
        return graph.AddField(field);
    }

    public FieldType AddSingleField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, object>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class
    {
        var field = BuildSingleField(name, resolve, mutate, arguments, graphType, nullable, description);
        return graph.AddField(field);
    }

    public FieldType AddSingleField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
        Type? graphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        bool nullable = false,
        string? description = null)
        where TReturn : class
    {
        var field = BuildSingleField(name, resolve, mutate, arguments, graphType, nullable, description);
        return graph.AddField(field);
    }

    FieldType BuildSingleField<TSource, TReturn>(
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
        Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate,
        IEnumerable<QueryArgument>? arguments,
        Type? graphType,
        bool nullable,
        string? description)
        where TReturn : class
    {
        Guard.AgainstWhiteSpace(nameof(name), name);

        graphType ??= GraphTypeFinder.FindGraphType<TReturn>(nullable);

        var hasId = keyNames.ContainsKey(typeof(TReturn));
        return new()
        {
            Name = name,
            Type = graphType,
            Description = description,

            Arguments = ArgumentAppender.GetQueryArguments(arguments, hasId, false),

            Resolver = new FuncFieldResolver<TSource, TReturn?>(
                async context =>
                {
                    var efFieldContext = BuildContext(context);

                    var names = GetKeyNames<TReturn>();

                    var query = resolve(efFieldContext);
                    if (disableTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    query = includeAppender.AddIncludes(query, context);
                    query = query.ApplyGraphQlArguments(context, names, false);

                    QueryLogger.Write(query);

                    TReturn? single;
                    if (disableAsync)
                    {
                        single = query.SingleOrDefault();
                    }
                    else
                    {
                        single = await query.SingleOrDefaultAsync(context.CancellationToken);
                    }

                    if (single is not null)
                    {
                        if (await efFieldContext.Filters.ShouldInclude(context.UserContext, single))
                        {
                            if (mutate is not null)
                            {
                                await mutate.Invoke(efFieldContext, single);
                            }

                            return single;
                        }
                    }

                    if (nullable)
                    {
                        return null;
                    }

                    throw new ExecutionError("Not found");
                })
        };
    }
}