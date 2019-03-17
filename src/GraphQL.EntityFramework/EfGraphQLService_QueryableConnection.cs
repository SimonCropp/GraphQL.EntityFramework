using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public void AddQueryConnectionField<TGraph, TReturn>(ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildQueryConnectionField<object, TGraph, TReturn>(name, resolve, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildQueryConnectionField<TSource, TGraph, TReturn>(name, resolve, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TGraph, TReturn>(ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildQueryConnectionField<TSource, TGraph, TReturn>(name, resolve, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
        }

        ConnectionBuilder<TGraph, TSource> BuildQueryConnectionField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IQueryable<TReturn>> resolve,
            int pageSize)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            Guard.AgainstNegative(nameof(pageSize), pageSize);
            var builder = ConnectionBuilder.Create<TGraph, TSource>();
            builder.PageSize(pageSize);
            //todo:
            //builder.Bidirectional();
            builder.Name(name);
            builder.Resolve(
                context =>
                {
                    var withIncludes = includeAppender.AddIncludes(resolve(context), context);
                    var withArguments = withIncludes.ApplyGraphQlArguments(context);
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