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
        public void AddQueryConnectionField<TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var connection = BuildQueryConnectionField(name, resolve, pageSize, itemGraphType, description);

            var field = graph.AddField(connection.FieldType);

            var hasId = keyNames.ContainsKey(typeof(TReturn));
            field.AddWhereArgument(hasId, arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            IComplexGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var connection = BuildQueryConnectionField(name, resolve, pageSize, itemGraphType, description);

            var field = graph.AddField(connection.FieldType);

            var hasId = keyNames.ContainsKey(typeof(TReturn));
            field.AddWhereArgument(hasId,arguments);
        }

        ConnectionBuilder<TSource> BuildQueryConnectionField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>>? resolve,
            int pageSize,
            Type? itemGraphType,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNegative(nameof(pageSize), pageSize);

            itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            var fieldType = GetFieldType<TSource>(name, itemGraphType);

            var builder = ConnectionBuilder<TSource>.Create<FakeGraph>(name);
            if (description != null)
            {
                builder.Description(description);
            }
            builder.PageSize(pageSize).Bidirectional();
            SetField(builder, fieldType);

            if (resolve != null)
            {
                builder.Resolve(
                    context =>
                    {
                        var efFieldContext = BuildContext(context);
                        var query = resolve(efFieldContext);
                        if (disableTracking)
                        {
                            query = query.AsNoTracking();
                        }
                        query = includeAppender.AddIncludes(query, context);
                        var names = GetKeyNames<TReturn>();
                        query = query.ApplyGraphQlArguments(context, names);
                        return query
                            .ApplyConnectionContext(
                                context.First,
                                context.After,
                                context.Last,
                                context.Before,
                                context,
                                context.CancellationToken,
                                efFieldContext.Filters);
                    });
            }

            return builder;
        }
    }
}