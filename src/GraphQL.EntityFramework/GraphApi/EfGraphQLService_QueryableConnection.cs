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
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            //build the connection field
            var connection = BuildQueryConnectionField(name, resolve, pageSize, graphType);
            //add the field to the graph
            var field = graph.AddField(connection.FieldType);
            //append the optional where arguments to the field
            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            ObjectGraphType graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            //build the connection field
            var connection = BuildQueryConnectionField(name, resolve, pageSize, graphType);
            //add the field to the graph
            var field = graph.AddField(connection.FieldType);
            //append the optional where arguments to the field
            field.AddWhereArgument(arguments);
        }

        public void AddQueryConnectionField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            //build the connection field
            var connection = BuildQueryConnectionField(name, resolve, pageSize, graphType);
            //add the field to the graph
            var field = graph.AddField(connection.FieldType);
            //append the optional where arguments to the field
            field.AddWhereArgument(arguments);
        }

        ConnectionBuilder<FakeGraph, TSource> BuildQueryConnectionField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IQueryable<TReturn>> resolve,
            int pageSize,
            Type? graphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            Guard.AgainstNegative(nameof(pageSize), pageSize);

            //lookup the graph type if not explicitly specified
            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            //create a ConnectionBuilder<graphType, TSource> type by invoking the static Create method on the generic type
            var fieldType = GetFieldType<TSource>(name, graphType);
            //create a ConnectionBuilder<FakeGraph, TSource> which will be returned from this method
            var builder = ConnectionBuilder<FakeGraph, TSource>.Create(name);
            //set the page size
            builder.PageSize(pageSize);
            //using reflection, override the private field type property of the ConnectionBuilder<FakeGraph, TSource> to be the ConnectionBuilder<graphType, TSource> object
            SetField(builder, fieldType);

            //set the resolve function (note: this is not async capable)
            builder.Resolve(
                context =>
                {
                    //obtain the ef context
                    var efFieldContext = BuildContext(context);
                    //run the resolve function, then include the related tables on the returned query
                    var withIncludes = includeAppender.AddIncludes(resolve(efFieldContext), context);
                    //get field names of the table's primary key(s)
                    var names = GetKeyNames<TReturn>();
                    //apply any query filters specified in the arguments
                    var withArguments = withIncludes.ApplyGraphQlArguments(context, names);
                    //apply skip/take to query as appropriate, and return the query
                    return withArguments
                        .ApplyConnectionContext(
                            context.First,
                            context.After,
                            context.Last,
                            context.Before,
                            context,
                            context.CancellationToken,
                            efFieldContext.Filters);
                    //note: does not apply global filters
                });

            //return the field to be added to the graph
            return builder;
        }
    }
}