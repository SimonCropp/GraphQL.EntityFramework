using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace EfCoreGraphQL
{
    public static partial class ObjectGraphExtension
    {
        //todo: add a where to make TGraphType and match TReturnType
        public static FieldType AddQueryField<TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<object>, IQueryable<TReturnType>> resolve,
            string includeName = null)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var field = BuildQueryField<object, TGraphType, TReturnType>(name, resolve,   includeName);
            return graphType.AddField(field);
        }

        public static FieldType AddQueryField<TSourceType, TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IQueryable<TReturnType>> resolve,
            string includeName = null)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var field = BuildQueryField<TSourceType, TGraphType, TReturnType>(name, resolve,   includeName);
            return graphType.AddField(field);
        }

        static FieldType BuildQueryField<TSourceType, TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IQueryable<TReturnType>> resolve,
            string includeName)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var field = new FieldType
            {
                Name = name,
                Type = typeof(ListGraphType<TGraphType>),
                Arguments = ArgumentAppender.GetQueryArguments(),
                Resolver = new AsyncFieldResolver<TSourceType, List<TReturnType>>(
                    async context =>
                    {
                        var returnTypes = resolve(context);
                        return await
                            IncludeAppender.AddIncludes(returnTypes, context)
                                .ApplyGraphQlArguments(context)
                                .ToListAsync()
                                .ConfigureAwait(false);
                    })
            };

            field.SetIncludeMetadata(includeName);
            return field;
        }
    }
}