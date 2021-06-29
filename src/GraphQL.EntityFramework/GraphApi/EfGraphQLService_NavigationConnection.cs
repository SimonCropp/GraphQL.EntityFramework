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
        static MethodInfo addEnumerableConnection = typeof(EfGraphQLService<TDbContext>)
            .GetMethod("AddEnumerableConnection", BindingFlags.Instance| BindingFlags.NonPublic)!;

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
            Guard.AgainstWhiteSpace(nameof(name), name);
            Guard.AgainstNegative(nameof(pageSize), pageSize);

            itemGraphType ??= GraphTypeFinder.FindGraphType<TReturn>();

            var addConnectionT = addEnumerableConnection.MakeGenericMethod(typeof(TSource), itemGraphType, typeof(TReturn));
            addConnectionT.Invoke(this, new object?[] { graph, name, resolve, pageSize, description, arguments, includeNames });
        }

        void AddEnumerableConnection<TSource, TGraph, TReturn>(
            ComplexGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve,
            int pageSize,
            string? description,
            IEnumerable<QueryArgument>? arguments,
            IEnumerable<string>? includeNames)
            where TGraph : IGraphType
            where TReturn : class
        {
            var builder = ConnectionBuilder<TSource>.Create<TGraph>(name);

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

            var field = graph.AddField(builder.FieldType);

            field.AddWhereArgument(hasId, arguments);
        }
    }
}