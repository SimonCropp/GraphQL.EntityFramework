using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return AddSingleField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, nullable);
        }

        public FieldType AddSingleField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType, nullable);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return AddSingleField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, nullable);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType, nullable);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            return AddSingleField<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, nullable);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, arguments, graphType, nullable);
            return graph.AddField(field);
        }

        FieldType BuildSingleField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument>? arguments,
            Type? graphType,
            bool nullable)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);


            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            Type? wrappedType;
            if (nullable)
            {
                wrappedType = graphType;
            }
            else
            {
                wrappedType = typeof(NonNullGraphType<>).MakeGenericType(graphType);
            }

            return new FieldType
            {
                Name = name,
                Type = wrappedType,

                Arguments = ArgumentAppender.GetQueryArguments(arguments),

                Resolver = new AsyncFieldResolver<TSource, TReturn?>(
                    async context =>
                    {
                        var efFieldContext = BuildContext(context);

                        var names = GetKeyNames<TReturn>();

                        var returnTypes = await resolve(efFieldContext);
                        var withIncludes = includeAppender.AddIncludes(returnTypes, context);
                        var withArguments = withIncludes.ApplyGraphQlArguments(context, names);
                        var single = await withArguments.SingleOrDefaultAsync(context.CancellationToken);
                        if (single != null)
                        {
                            if (await efFieldContext.Filters.ShouldInclude(context.UserContext, single))
                            {
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
}