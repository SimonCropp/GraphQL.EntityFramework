using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService
    {
        public ConnectionBuilder<TGraph, object> AddNavigationConnectionField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10,
            Filter<object, TReturn> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildListConnectionField<object, TGraph, TReturn>(name, resolve, includeNames, pageSize, filter);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        public ConnectionBuilder<TGraph, TSource> AddNavigationConnectionField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            IEnumerable<string> includeNames = null,
            int pageSize = 10,
            Filter<TSource, TReturn> filter = null)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            var connection = BuildListConnectionField<TSource, TGraph, TReturn>(name, resolve, includeNames, pageSize, filter);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        ConnectionBuilder<TGraph, TSource> BuildListConnectionField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string> includeName,
            int pageSize,
            Filter<TSource, TReturn> filter)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            Guard.AgainstNegative(nameof(pageSize), pageSize);
            var builder = ConnectionBuilder.Create<TGraph, TSource>();
            builder.PageSize(pageSize);
            builder.Name(name);
            IncludeAppender.SetIncludeMetadata(builder.FieldType, name, includeName);
            builder.Resolve(context =>
            {
                var enumerable = resolve(context);
                enumerable = enumerable.ApplyGraphQlArguments(context);
                if (filter != null)
                {
                    enumerable = enumerable.Where(x => filter(context, x));
                }
                var page = enumerable.ToList();

                return ConnectionConverter.ApplyConnectionContext(
                    page,
                    context.First,
                    context.After,
                    context.Last,
                    context.Before);
            });
            return builder;
        }
    }
}