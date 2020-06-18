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
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var connection = BuildListConnectionField(name, resolve, includeNames, pageSize, itemGraphType);

            var field = graph.AddField(connection.FieldType);

            field.AddWhereArgument(arguments);
        }

        ConnectionBuilder<FakeGraph, TSource> BuildListConnectionField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            IEnumerable<string>? includeName,
            int pageSize,
            Type? itemGraphType)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNull(nameof(resolve), resolve);
            Guard.AgainstNegative(nameof(pageSize), pageSize);

            itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            var fieldType = GetFieldType<TSource>(name, itemGraphType);

            var builder = ConnectionBuilder<FakeGraph, TSource>.Create(name);

            builder.PageSize(pageSize);
            SetField(builder, fieldType);
            IncludeAppender.SetIncludeMetadata(builder.FieldType, name, includeName);

            builder.ResolveAsync(async context =>
            {
                var efFieldContext = BuildContext(context);

                var enumerable = resolve(efFieldContext);

                enumerable = enumerable.ApplyGraphQlArguments(context);
                enumerable = await efFieldContext.Filters.ApplyFilter(enumerable, context.UserContext);
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

        static void SetField(object builder, object fieldType)
        {
            var fieldTypeField = builder.GetType().GetProperty("FieldType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            fieldTypeField.SetValue(builder, fieldType);
        }

        static object GetFieldType<TSource>(string name, Type itemGraphType)
        {
            var makeGenericType = typeof(ConnectionBuilder<,>).MakeGenericType(itemGraphType, typeof(TSource));
            dynamic x = makeGenericType.GetMethod("Create").Invoke(null, new object[] { name });
            return x.FieldType;
        }

        class FakeGraph : GraphType
        {
        }
    }
}