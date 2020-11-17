using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public void AddQueryPaginationField<TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
               int page = 1,
            int row = 50,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var pagination = BuildQueryPaginationField(name, resolve, page, row, itemGraphType, description);

            var field = graph.AddField(pagination.FieldType);

            field.AddWhereArgument(arguments);
        }

        public void AddQueryPaginationField<TSource, TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int page = 1,
            int row = 50,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var pagination = BuildQueryPaginationField(name, resolve, page, row, itemGraphType, description);

            var field = graph.AddField(pagination.FieldType);

            field.AddWhereArgument(arguments);
        }

        PaginationBuilder<TSource> BuildQueryPaginationField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve,
            int page,
            int row,
            Type? itemGraphType,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNegative(nameof(page), page);
            Guard.AgainstNegative(nameof(row), row);

            itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            var fieldType = GetPaginationFieldType<TSource>(name, itemGraphType);

            var builder = PaginationBuilder<TSource>.Create<FakeGraph>(name);
            if (description != null)
            {
                builder.Description(description);
            }
            builder.Page(page).Row(row);
            SetField(builder, fieldType);

            if (resolve != null)
            {
                builder.Resolve(
                    context =>
                    {
                        var efFieldContext = BuildContext(context);
                        var query = resolve(efFieldContext);
                        query = includeAppender.AddIncludes(query, context);
                        var names = GetKeyNames<TReturn>();
                        query = query.ApplyGraphQlArguments(context, names);
                        return query
                            .ApplyPaginationContext(
                                context.Page,
                                context.Row,
                                context,
                                context.CancellationToken,
                                efFieldContext.Filters);
                    });
            }

            return builder;
        }
        
        
        //TODO: can return null
        static object GetPaginationFieldType<TSource>(string name, Type graphType)
        {
            var makeGenericType = typeof(PaginationBuilder<>).MakeGenericType(typeof(TSource));
            var genericMethodInfo = makeGenericType.GetMethods().Single(mi => mi.Name == "Create" && mi.IsGenericMethod && mi.GetGenericArguments().Length == 1);
            var genericMethod = genericMethodInfo.MakeGenericMethod(graphType);
            dynamic? x = genericMethod.Invoke(null, new object[] { name }) ?? null;
            return x?.FieldType!;
        }
    }
}