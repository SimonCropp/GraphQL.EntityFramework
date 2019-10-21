using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL.Builders;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public void AddNavigationConnectionField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? graphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            //build the connection field
            var connection = BuildListConnectionField(name, resolve, includeNames, pageSize, graphType);
            //add the field to the graph
            var field = graph.AddField(connection.FieldType);
            //append the optional where arguments to the field
            field.AddWhereArgument(arguments);
        }

        ConnectionBuilder<FakeGraph, TSource> BuildListConnectionField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string>? includeName,
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
            //add the metadata for the tables to be included in the query to the ConnectionBuilder<graphType, TSource> object
            IncludeAppender.SetIncludeMetadata(builder.FieldType, name, includeName);
            //set the custom resolver
            builder.ResolveAsync(async context =>
            {
                var efFieldContext = BuildContext(context);
                //run the specified resolve function
                var enumerable = resolve(efFieldContext);
                //apply any query filters specified in the arguments
                enumerable = enumerable.ApplyGraphQlArguments(context);
                //apply the global filter on each individually enumerated item
                enumerable = await efFieldContext.Filters.ApplyFilter(enumerable, context.UserContext);
                //pagination does NOT occur server-side at this point, as the query has already executed
                var page = enumerable.ToList();
                //return the proper page of data
                return ConnectionConverter.ApplyConnectionContext(
                    page,
                    context.First,
                    context.After,
                    context.Last,
                    context.Before);
            });

            //return the field to be added to the graph
            return builder;
        }

        static void SetField(object builder, object fieldType)
        {
            var fieldTypeField = builder.GetType().GetProperty("FieldType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            fieldTypeField.SetValue(builder, fieldType);
        }

        static object GetFieldType<TSource>(string name, Type graphType)
        {
            var makeGenericType = typeof(ConnectionBuilder<,>).MakeGenericType(graphType, typeof(TSource));
            dynamic x = makeGenericType.GetMethod("Create").Invoke(null, new object[] { name });
            return x.FieldType;
        }

        class FakeGraph : GraphType
        {
        }
    }
}