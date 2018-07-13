using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace EfCoreGraphQL
{
    public static class ObjectGraphExtension
    {
        public static FieldType AddEfListField<TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturnType>> resolve,
            string description = null,
            QueryArguments arguments = null,
            string deprecationReason = null)
            where TGraphType : IGraphType where TReturnType : class
        {
            return AddEfListField<object, TGraphType, TReturnType>(graphType, name, resolve, description, arguments, deprecationReason);
        }

        public static FieldType AddEfListField<TSourceType, TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IQueryable<TReturnType>> resolve,
            string description = null,
            QueryArguments arguments = null,
            string deprecationReason = null)
            where TGraphType : IGraphType where TReturnType : class
        {
            arguments = GetQueryArguments(arguments);

            var fieldType = new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = typeof(ListGraphType<TGraphType>),
                Arguments = arguments,
                Resolver = new AsyncFieldResolver<TSourceType, List<TReturnType>>(async context =>
                    {
                        var returnTypes = resolve(context);
                        return await
                            IncludeAppender.AddIncludes(returnTypes, context)
                            .ApplyGraphQlArguments(context)
                            .ToListAsync()
                            .ConfigureAwait(false);
                    })
            };
            return graphType.AddField(fieldType);
        }

        static QueryArguments GetQueryArguments(QueryArguments arguments)
        {
            if (arguments == null)
            {
                return ArgumentAppender.DefaultArguments;
            }

            arguments.AddGraphQlArguments();

            return arguments;
        }
    }
}