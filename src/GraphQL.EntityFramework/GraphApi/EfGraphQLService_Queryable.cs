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
        public FieldType AddQueryField<TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
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
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
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
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var hasId = keyNames.ContainsKey(typeof(TReturn));
            FieldType fieldType = new()
            {
                Name = name,
                Description = description,
                Type = MakeListGraphType<TReturn>(itemGraphType),
                Arguments = ArgumentAppender.GetQueryArguments(arguments,hasId),
            };

            if (resolve != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
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
                        query = query.ApplyGraphQlArguments(context, names);

                        var list = await query
                            .ToListAsync(context.CancellationToken);
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
            if (itemGraphType == null)
            {
                return typeof(IEnumerable<TReturn>).GetGraphTypeFromType();
            }
            return nonNullType.MakeGenericType(listGraphType.MakeGenericType(itemGraphType));
        }

        List<string>? GetKeyNames<TSource>()
        {
            if (keyNames.TryGetValue(typeof(TSource), out var names))
            {
                return names;
            }

            return null;
        }
    }
}