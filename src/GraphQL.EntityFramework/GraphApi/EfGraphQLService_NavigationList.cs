using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddNavigationListField<TSource, TReturn>(
            ObjectGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>> resolve,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);
            Guard.AgainstNull(nameof(resolve), resolve);

            var field = BuildNavigationField(itemGraphType, name, resolve, includeNames, arguments, description);
            return graph.AddField(field);
        }

        public FieldType AddNavigationListField<TSource, TReturn>(
            InterfaceGraphType<TSource> graph,
            string name,
            Type? itemGraphType = null,
            IEnumerable<QueryArgument>? arguments = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstNull(nameof(graph), graph);

            var field = BuildNavigationField<TSource, TReturn>(itemGraphType, name, null, includeNames, arguments, description);
            return graph.AddField(field);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            Type? itemGraphType,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve,
            IEnumerable<string>? includeNames,
            IEnumerable<QueryArgument>? arguments,
            string? description)
            where TReturn : class
        {
            var listGraphType = MakeListGraphType<TReturn>(itemGraphType);
            return BuildNavigationField(name, resolve, includeNames, listGraphType, arguments, description);
        }

        FieldType BuildNavigationField<TSource, TReturn>(
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, IEnumerable<TReturn>>? resolve,
            IEnumerable<string>? includeNames,
            Type listGraphType,
            IEnumerable<QueryArgument>? arguments,
            string? description)
            where TReturn : class
        {
            Guard.AgainstNullWhiteSpace(nameof(name), name);

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                Type = listGraphType,
                Arguments = ArgumentAppender.GetQueryArguments(arguments),
                Metadata = IncludeAppender.GetIncludeMetadata(name, includeNames)
            };

            if (resolve != null)
            {
                fieldType.Resolver = new AsyncFieldResolver<TSource, IEnumerable<TReturn>>(
                    context =>
                    {
                        var efFieldContext = BuildContext(context);
                        var result = resolve(efFieldContext);
                        result = result.ApplyGraphQlArguments(context);
                        return efFieldContext.Filters.ApplyFilter(result, context.UserContext);
                    });
            }

            return fieldType;
        }
    }
}