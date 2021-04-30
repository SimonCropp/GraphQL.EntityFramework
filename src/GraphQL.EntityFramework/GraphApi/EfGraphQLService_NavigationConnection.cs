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
            ComplexGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve = null,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            int pageSize = 10,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            Guard.AgainstNullWhiteSpace(nameof(name), name);
            Guard.AgainstNegative(nameof(pageSize), pageSize);

            itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();
            var fieldType = GetFieldType<TSource>(name, itemGraphType);

            var builder = ConnectionBuilder<TSource>.Create<FakeGraph>(name);
            SetField(builder, fieldType);
            if (description != null)
            {
                builder.Description(description);
            }

            builder.PageSize(pageSize).Bidirectional();
            IncludeAppender.SetIncludeMetadata(builder.FieldType, name, includeNames);

            var hasId = keyNames.ContainsKey(typeof(TReturn));
            if (resolve != null)
            {
                builder.ResolveAsync(async context =>
                {
                    var efFieldContext = BuildContext(context);

                    var enumerable = resolve(efFieldContext);

                    enumerable = enumerable.ApplyGraphQlArguments(hasId, context);
                    enumerable = await efFieldContext.Filters.ApplyFilter(enumerable, context.UserContext);
                    var page = enumerable.ToList();

                    return ConnectionConverter.ApplyConnectionContext(
                        page,
                        context.First,
                        context.After,
                        context.Last,
                        context.Before);
                });
            }

            var connection = builder;

            var field = graph.AddField(connection.FieldType);

            field.AddWhereArgument(hasId, arguments);
        }

        static void SetField(object builder, object fieldType)
        {
            var fieldTypeField = builder.GetType()
                .GetProperty("FieldType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!;
            fieldTypeField.SetValue(builder, fieldType);
        }

        //TODO: can return null
        static object GetFieldType<TSource>(string name, Type graphType)
        {
            var makeGenericType = typeof(ConnectionBuilder<>).MakeGenericType(typeof(TSource));
            var genericMethodInfo = makeGenericType.GetMethods().Single(mi => mi.Name == "Create" && mi.IsGenericMethod && mi.GetGenericArguments().Length == 1);
            var genericMethod = genericMethodInfo.MakeGenericMethod(graphType);
            dynamic? x = genericMethod.Invoke(null, new object[] {name}) ?? null;
            x?.Bidirectional();
            return x?.FieldType!;
        }

        class FakeGraph : GraphType
        {
        }
    }
}