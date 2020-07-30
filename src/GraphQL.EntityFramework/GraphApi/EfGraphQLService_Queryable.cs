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
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return AddQueryField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, description);
        }

        public FieldType AddQueryField<TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return AddQueryField(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, description);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolve, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            return AddQueryField<TSource, TReturn>(graph, name, x => Task.FromResult(resolve(x)), graphType, arguments, description);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(itemGraphType, name, resolve, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField<TSource, TReturn>(name, null, arguments, itemGraphType, description);
            return graph.AddField(field);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            Type? itemGraphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>> resolve,
            IEnumerable<QueryArgument>? arguments,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(resolve), resolve);

            return BuildQueryField(name, resolve, arguments, itemGraphType, description);
        }

        FieldType BuildQueryField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>>? resolve,
            IEnumerable<QueryArgument>? arguments,
            Type? itemGraphType,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var listGraphType = MakeListGraphType<TReturn>(itemGraphType);

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
            };

            if (resolve != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var efFieldContext = BuildContext(context);
                        var names = GetKeyNames<TReturn>();
                        var query = await resolve(efFieldContext);
                        query = includeAppender.AddIncludes(query, context);
                        query = query.ApplyGraphQlArguments(context, names);

                        var list = await query.ToListAsync(context.CancellationToken);
                        return await efFieldContext.Filters.ApplyFilter(list, context.UserContext);
                    });
            }

            return fieldType;
        }

        static Type listGraphType = typeof(ListGraphType<>);
        static Type nonNullType = typeof(NonNullGraphType<>);

        static Type MakeListGraphType<TReturn>(Type? itemGraphType)
            where TReturn : class
        {
            if (itemGraphType == null)
            {
                return typeof(IEnumerable<TReturn>).GetGraphTypeFromType();
            }
            return nonNullType.MakeGenericType(listGraphType.MakeGenericType(itemGraphType));
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