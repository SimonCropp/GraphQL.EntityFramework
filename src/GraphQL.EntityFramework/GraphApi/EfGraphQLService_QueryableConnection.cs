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
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, object>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildQueryConnectionField(name, resolve, pageSize, graphType);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildQueryConnectionField(name, resolve, pageSize, graphType);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type graphType = null,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildQueryConnectionField(name, resolve, pageSize, graphType);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
        }

        ConnectionBuilder<FakeGraph, TSource> BuildQueryConnectionField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            int pageSize,
            Type graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            Guard.AgainstNegative(nameof(pageSize), pageSize);

            graphType = GraphTypeFinder.FindGraphType<TReturn>(graphType);
            var fieldType = GetFieldType<TSource>(name, graphType);
            var builder = ConnectionBuilder<FakeGraph, TSource>.Create(name);
            builder.PageSize(pageSize);
            SetField(builder, fieldType);

            builder.Resolve(
                context =>
                {
                    var efFieldContext = BuildEfContextFromGraphQlContext(context);
                    var withIncludes = includeAppender.AddIncludes(resolve(efFieldContext), context);
                    var names = GetKeyNames<TReturn>();
                    var withArguments = withIncludes.ApplyGraphQlArguments(context, names);
                    return withArguments
                        .ApplyConnectionContext(
                            context.First,
                            context.After,
                            context.Last,
                            context.Before,
                            context,
                            context.CancellationToken,
                            filters);
                });
            return builder;
        }
    }
}