﻿using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    partial class EfGraphQLService<TDbContext>
        where TDbContext : DbContext
    {
        public FieldType AddNavigationField<TSource, TReturn>(
            ComplexGraphType<TSource> graph,
            string name,
            Func<ResolveEfFieldContext<TDbContext, TSource>, TReturn?>? resolve = null,
            Type? graphType = null,
            IEnumerable<string>? includeNames = null,
            string? description = null)
            where TReturn : class
        {
            Guard.AgainstWhiteSpace(nameof(name), name);

            graphType ??= GraphTypeFinder.FindGraphType<TReturn>();

            FieldType field = new()
            {
                Name = name,
                Type = graphType,
                Description = description
            };
            IncludeAppender.SetIncludeMetadata(field, name, includeNames);

            if (resolve != null)
            {
                field.Resolver = new AsyncFieldResolver<TSource, TReturn?>(
                    async context =>
                    {
                        var fieldContext = BuildContext(context);

                        var result = resolve(fieldContext);
                        if (await fieldContext.Filters.ShouldInclude(context.UserContext, result))
                        {
                            return result;
                        }

                        return null;
                    });
            }

            return graph.AddField(field);
        }
    }
}