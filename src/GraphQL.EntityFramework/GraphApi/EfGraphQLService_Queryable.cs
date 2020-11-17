﻿using System;
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
        
        
        
        public FieldType AddQueryField<TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IQueryable<TReturn>>>? resolveAsync = null,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(graphType, name, resolveAsync, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddQueryField<TSource, TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>>? resolveAsync = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildQueryField(itemGraphType, name, resolveAsync, arguments, description);
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

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                Type = MakeListGraphType<TReturn>(itemGraphType),
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
            };

            if (resolve != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var fieldContext = BuildContext(context);
                        var names = GetKeyNames<TReturn>();
                        var query = resolve(fieldContext);
                        query = includeAppender.AddIncludes(query, context);
                        query = query.ApplyGraphQlArguments(context, names);

                        var list = await query.ToListAsync(context.CancellationToken);
                        return await fieldContext.Filters.ApplyFilter(list, context.UserContext);
                    });
            }

            return fieldType;
        }
        
        

        FieldType BuildQueryField<TSource, TReturn>(
            Type? itemGraphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IQueryable<TReturn>>>? resolveAsync,
            IEnumerable<QueryArgument>? arguments,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                Type = MakeListGraphType<TReturn>(itemGraphType),
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
            };

            if (resolveAsync != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    async context =>
                    {
                        var fieldContext = BuildContext(context);
                        var names = GetKeyNames<TReturn>();
                        var query = await resolveAsync(fieldContext);
                        query = includeAppender.AddIncludes(query, context);
                        query = query.ApplyGraphQlArguments(context, names);
                        var list = await query.ToListAsync(context.CancellationToken);
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