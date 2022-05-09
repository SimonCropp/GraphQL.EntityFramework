using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework;

partial class EfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    static MethodInfo addQueryableConnection = typeof(EfGraphQLService<TDbContext>)
        .GetMethod("AddQueryableConnection", BindingFlags.Instance| BindingFlags.NonPublic)!;

    public void AddQueryConnectionField<TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var addConnectionT = addQueryableConnection.MakeGenericMethod(typeof(object), itemGraphType, typeof(TReturn));
        addConnectionT.Invoke(this, new object?[] { graph, name, resolve, arguments, pageSize, description });
    }

    public void AddQueryConnectionField<TSource, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
        Type? itemGraphType = null,
        IEnumerable<QueryArgument>? arguments = null,
        int pageSize = 10,
        string? description = null)
        where TReturn : class
    {
        itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
        var addConnectionT = addQueryableConnection.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn));
        addConnectionT.Invoke(this, new object?[] { graph, name, resolve, arguments, pageSize, description });
    }

    void AddQueryableConnection<TSource, TGraph, TReturn>(
        IComplexGraphType graph,
        string name,
        Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve,
        IEnumerable<QueryArgument>? arguments,
        int pageSize,
        string? description)
        where TGraph : IGraphType
        where TReturn : class
    {
        var builder = ConnectionBuilder.Create<TGraph, TSource>();
        builder.Name(name);
        if (description is not null)
        {
            builder.Description(description);
        }
        builder.PageSize(pageSize).Bidirectional();

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
        field.AddWhereArgument(hasId, arguments);
    }

}