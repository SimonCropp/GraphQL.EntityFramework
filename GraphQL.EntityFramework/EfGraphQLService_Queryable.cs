using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

partial class EfGraphQLService
{
    public FieldType AddQueryField<TReturn>(
        ObjectGraphType graph,
        Type graphType,
        string name,
        Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments = null)
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graph), graph);
        var field = BuildQueryField(graphType, name, resolve, arguments);
        return graph.AddField(field);
    }

    public FieldType AddQueryField<TSource, TReturn>(
        ObjectGraphType<TSource> graph,
        Type graphType,
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments = null)
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graph), graph);
        var field = BuildQueryField(graphType, name, resolve, arguments);
        return graph.AddField(field);
    }

    public FieldType AddQueryField<TSource, TReturn>(
        ObjectGraphType graph,
        Type graphType,
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments = null)
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graph), graph);
        var field = BuildQueryField(graphType, name, resolve, arguments);
        return graph.AddField(field);
    }

    FieldType BuildQueryField<TSource, TReturn>(
        Type graphType,
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments)
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graphType), graphType);
        var listGraphType = MakeListGraphType(graphType);
        return BuildQueryField(name, resolve, arguments, listGraphType);
    }

    public FieldType AddQueryField<TGraph, TReturn>(
        ObjectGraphType graph,
        string name,
        Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments = null)
        where TGraph : ObjectGraphType<TReturn>, IGraphType
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graph), graph);
        var field = BuildQueryField<object, TGraph, TReturn>(name, resolve, arguments);
        return graph.AddField(field);
    }

    public FieldType AddQueryField<TSource, TGraph, TReturn>(
        ObjectGraphType graph,
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments = null,
        string includeName = null)
        where TGraph : ObjectGraphType<TReturn>, IGraphType
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graph), graph);
        var field = BuildQueryField<TSource, TGraph, TReturn>(name, resolve, arguments);
        return graph.AddField(field);
    }

    public FieldType AddQueryField<TSource, TGraph, TReturn>(
        ObjectGraphType<TSource> graph,
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments = null)
        where TGraph : ObjectGraphType<TReturn>, IGraphType
        where TReturn : class
    {
        Guard.AgainstNull(nameof(graph), graph);
        var field = BuildQueryField<TSource, TGraph, TReturn>(name, resolve, arguments);
        return graph.AddField(field);
    }

    FieldType BuildQueryField<TSource, TGraph, TReturn>(
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments)
        where TGraph : ObjectGraphType<TReturn>, IGraphType
        where TReturn : class
    {
        var listGraphType = MakeListGraphType(typeof(TGraph));
        return BuildQueryField(name, resolve, arguments, listGraphType);
    }

    FieldType BuildQueryField<TSource, TReturn>(
        string name,
        Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
        IEnumerable<QueryArgument> arguments,
        Type listGraphType)
        where TReturn : class
    {
        Guard.AgainstNullWhiteSpace(nameof(name), name);
        Guard.AgainstNull(nameof(resolve), resolve);
        return new FieldType
        {
            Name = name,
            Type = listGraphType,
            Arguments = ArgumentAppender.GetQueryArguments(arguments),
            Resolver = new FuncFieldResolver<TSource, Task<List<TReturn>>>(
                context =>
                {
                    var returnTypes = resolve(context);
                    var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                    var withArguments = withIncludes.ApplyGraphQlArguments(context);
                    return withArguments.ToListAsync();
                })
        };
    }
}