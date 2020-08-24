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
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, mutate, arguments, graphType, nullable, description);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            ComplexGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildSingleField(name, resolve, mutate, arguments, graphType, nullable, description);
            return graph.AddField(field);
        }

        public FieldType AddSingleField<TSource, TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn, Task>? mutate = null,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            bool nullable = false,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
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
                Description = description,

                Arguments = ArgumentAppender.GetQueryArguments(arguments),

                Resolver = new AsyncFieldResolver<TSource, TReturn?>(
                    async context =>
                    {
                        var efFieldContext = BuildContext(context);

                        var names = GetKeyNames<TReturn>();

                        var query = resolve(efFieldContext);
                        query = includeAppender.AddIncludes(query, context);
                        query = query.ApplyGraphQlArguments(context, names);
                        var single = await WrappedSingle(name, graphType, query, context);

                        if (single != null)
                        {
                            if (await efFieldContext.Filters.ShouldInclude(context.UserContext, single))
                            {
                                if (mutate != null)
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

        static async Task<TReturn?> WrappedSingle<TSource, TReturn>(string name, Type graphType, IQueryable<TReturn> query, IResolveFieldContext<TSource> context)
            where TReturn : class
        {
            //TODO: remove catch in EF 5 https://github.com/dotnet/efcore/issues/18742
            try
            {
                return await query.SingleOrDefaultAsync(context.CancellationToken);
            }
            catch (InvalidOperationException exception)
            {
                throw new Exception($"SingleOrDefaultAsync threw InvalidOperationException. This is likely due it returning more than one result. Name: {name}. GraphType: {graphType}", exception);
            }
        }
    }
}