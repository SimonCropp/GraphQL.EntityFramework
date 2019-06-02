using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext,TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext,TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType);
            return graph.AddField(field);
        }

        FieldType BuildSingleField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext,TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments,
            Type graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);

            graphType = GraphTypeFinder.FindGraphType<TReturn>(graphType);
            return new FieldType
            {
                Name = name,
                Type = graphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Resolver = new AsyncFieldResolver<TSource, TReturn>(
                    async context =>
                    {
                        var returnTypes = resolve(BuildEfContextFromGraphQlContext(context));
                        var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                        var names = GetKeyNames<TReturn>();
                        var withArguments = withIncludes.ApplyGraphQlArguments(context, names);

                        var single = await withArguments.SingleOrDefaultAsync(context.CancellationToken);
                        if (await filters.ShouldInclude(context.UserContext, single))
                        {
                            return single;
                        }

                        return null;
                    })
            };
        }

    }
}