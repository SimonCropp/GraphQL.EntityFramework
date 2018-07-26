using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Builders;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.EntityFramework
{
    public partial class EfGraphQLService
    {
        public ConnectionBuilder<TGraph, object> AddNavigationConnectionField<TGraph, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildListConnectionField<object, TGraph, TReturn>( name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        public ConnectionBuilder<TGraph, TSource> AddNavigationConnectionField<TSource, TGraph, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<QueryArgument> arguments = null,
            string includeName = null,
            int pageSize = 10)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var connection = BuildListConnectionField<TSource, TGraph, TReturn>( name, resolve, includeName, pageSize);
            var field = graph.AddField(connection.FieldType);
            field.AddWhereArgument(arguments);
            return connection;
        }

        ConnectionBuilder<TGraph, TSource> BuildListConnectionField<TSource, TGraph, TReturn>(
            string name,
            Func<ResolveFieldContext<TSource>, IEnumerable<TReturn>> resolve,
            string includeName,
            int pageSize)
            where TGraph : ObjectGraphType<TReturn>, IGraphType
            where TReturn : class
        {
            var builder = ConnectionBuilder.Create<TGraph, TSource>();
            builder.PageSize(pageSize);
            builder.Name(name);
            IncludeAppender.SetIncludeMetadata(builder.FieldType, includeName);
            builder.Resolve(context =>
            {
                return ((Func<Connection<TReturn>>) (() =>
                {
                    var enumerable = resolve(context);
                    var page = enumerable
                        .ApplyGraphQlArguments(context)
                        .ToList();

                    return ConnectionConverter.ApplyConnectionContext(
                        page,
                        context.First,
                        context.After,
                        context.Last,
                        context.Before);
                }))();
            });
            return builder;
        }
    }
}