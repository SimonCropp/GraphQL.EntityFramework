using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
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
            return BuildQueryField(name, resolve, arguments, graphType);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            Type graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            graphType = GraphTypeFinder.FindGraphType<TReturn>(graphType);
            var listGraphType = MakeListGraphType(graphType);
            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var returnTypes = resolve(context);
                        var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                        var withArguments = withIncludes.ApplyGraphQlArguments(context);
                        var list = await withArguments.ToListAsync(context.CancellationToken);
                        return await filters.ApplyFilter(list, context.UserContext);
                    })
            };
        }
    }
}