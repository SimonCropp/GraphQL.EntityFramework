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
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            Guard.AgainstNull(nameof(resolve), resolve);

            var connection = BuildQueryConnectionField(name, resolve, pageSize, itemGraphType, description);

            var field = graph.AddField(connection.FieldType);

            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            IObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            Guard.AgainstNull(nameof(resolve), resolve);

            var connection = BuildQueryConnectionField(name, resolve, pageSize, itemGraphType, description);

            var field = graph.AddField(connection.FieldType);

            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            Guard.AgainstNull(nameof(resolve), resolve);

            var connection = BuildQueryConnectionField(name, resolve, pageSize, itemGraphType, description);

            var field = graph.AddField(connection.FieldType);

            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var connection = BuildQueryConnectionField<TSource, TReturn>(name, null, pageSize, itemGraphType, description);

            var field = graph.AddField(connection.FieldType);

            field.AddWhereArgument(arguments);
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
            builder.PageSize(pageSize);
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