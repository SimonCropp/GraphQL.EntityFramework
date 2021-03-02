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
        public FieldType AddUnionQueryField<TUnion>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IDictionary<Type, IQueryable<object>>>? resolve = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null) where TUnion : UnionGraphType
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildUnionQueryField<TUnion, object>( name, resolve, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddUnionQueryField<TUnion, TSource>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IDictionary<Type, IQueryable<object>>>? resolve = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null) where TUnion : UnionGraphType
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildUnionQueryField<TUnion, TSource>( name, resolve, arguments, description);
            return graph.AddField(field);
        }

        
        public FieldType AddUnionQueryField<TUnion>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, Task<IDictionary<Type, IQueryable<object>>>>? resolveAsync = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null) where TUnion : UnionGraphType
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildUnionQueryField<TUnion, object>( name, resolveAsync, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddUnionQueryField<TUnion, TSource>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IDictionary<Type, IQueryable<object>>>>? resolveAsync = null,
            IEnumerable<QueryArgument>? arguments = null,
            string? description = null) where TUnion : UnionGraphType
        {
            Guard.AgainstNull(nameof(graph), graph);
            var field = BuildUnionQueryField<TUnion, TSource>( name, resolveAsync, arguments, description);
            return graph.AddField(field);
        }



        




        FieldType BuildUnionQueryField<TUnion,TSource>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IDictionary<Type, IQueryable<object>>>? resolve,
            IEnumerable<QueryArgument>? arguments,
            string? description)where TUnion : UnionGraphType
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                Type = listGraphType.MakeGenericType(typeof(TUnion)),
                Arguments = new QueryArguments(arguments?? new List<QueryArgument>()),
            };

            if (resolve != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<object>>(
                    async context =>
                    {
                        var fieldContext = BuildContext(context);
                        var queries = resolve(fieldContext);
                        var list = new List<object>();

                        foreach (var query in queries)
                        {
                            // var names = GetKeyNames(query.Key);
                            var newQuery = includeAppender.AddUnionIncludes(query.Value, context, query.Key);
                            // newQuery = newQuery.ApplyGraphQlArguments(context, names);
                            list.AddRange(await newQuery.ToListAsync(context.CancellationToken));
                        }

                        return await fieldContext.Filters.ApplyFilter(list, context.UserContext);
                    });
                
            }

            return fieldType;
        }
        
        
        

        FieldType BuildUnionQueryField<TUnion,TSource>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, Task<IDictionary<Type, IQueryable<object>>>>? resolveAsync,
            IEnumerable<QueryArgument>? arguments,
            string? description)where TUnion : UnionGraphType
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                Type = listGraphType.MakeGenericType(typeof(TUnion)),
                Arguments = new QueryArguments(arguments?? new List<QueryArgument>()),
            };

            if (resolveAsync != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<object>>(
                    async context =>
                    {
                        var fieldContext = BuildContext(context);
                        var queries = await resolveAsync(fieldContext);
                        var list = new List<object>();

                        foreach (var query in queries)
                        {
                            // var names = GetKeyNames(query.Key);
                            var newQuery = includeAppender.AddUnionIncludes(query.Value, context, query.Key);
                            // newQuery = newQuery.ApplyGraphQlArguments(context, names);
                            list.AddRange(await newQuery.ToListAsync(context.CancellationToken));
                        }

                        return await fieldContext.Filters.ApplyFilter(list, context.UserContext);
                    });
                
            }

            return fieldType;
        }
    }
}