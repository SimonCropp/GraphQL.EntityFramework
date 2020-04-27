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
        public FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return AddQueryField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddQueryField<TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return AddQueryField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            return AddQueryField<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments);
            return graph.AddField(field);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            Type? graphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument>? arguments)
            where TReturn : class
        {
            return BuildQueryField(name, resolve, arguments, graphType);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument>? arguments,
            Type? graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);

            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

            var listGraphType = MakeListGraphType(graphType);

            return new FieldType
            {
                Name = name,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),

                Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var efFieldContext = BuildContext(context);
                        var names = GetKeyNames<TReturn>();
                        var query = await resolve(efFieldContext);
                        query = includeAppender.AddIncludes(query, context);
                        query = query.ApplyGraphQlArguments(context, names);

                        var list = await query.ToListAsync(context.CancellationToken);
                        return await efFieldContext.Filters.ApplyFilter(list, context.UserContext);
                    })
            };
        }

        static List<string> emptyList = new List<string>();

        List<string> GetKeyNames<TSource>()
        {
            if (keyNames.TryGetValue(typeof(TSource), out var names))
            {
                return names;
            }

            return emptyList;
        }
    }
}