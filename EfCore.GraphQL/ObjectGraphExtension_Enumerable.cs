using System;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static partial class ObjectGraphExtension
    {
        public static FieldType AddEnumerableField<TGraphType, TReturnType>(
            this ObjectGraphType graphType,
            string name,
            Func<ResolveFieldContext<object>, IEnumerable<TReturnType>> resolve,
            string includeName = null,
            string description = null,
            QueryArguments arguments = null,
            string deprecationReason = null)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var field = BuildEnumerableField<object, TGraphType, TReturnType>(name, resolve, description, arguments, deprecationReason);
            return graphType.AddField(field);
        }

        public static FieldType AddEnumerableField<TSourceType, TGraphType, TReturnType>(
            this ObjectGraphType<TSourceType> graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string description = null,
            QueryArguments arguments = null,
            string deprecationReason = null)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            var field = BuildEnumerableField<TSourceType, TGraphType, TReturnType>(name, resolve, description, arguments, deprecationReason);
            return graphType.AddField(field);
        }

        static FieldType BuildEnumerableField<TSourceType, TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>,
            IEnumerable<TReturnType>> resolve,
            string description,
            QueryArguments arguments,
            string deprecationReason)
            where TGraphType : IGraphType
            where TReturnType : class
        {
            arguments = GetQueryArguments(arguments);

            var field = new FieldType
            {
                Name = name,
                Description = description,
                DeprecationReason = deprecationReason,
                Type = typeof(ListGraphType<TGraphType>),
                Arguments = arguments,
                Resolver = new FuncFieldResolver<TSourceType, IEnumerable<TReturnType>>(
                    context =>
                    {
                        var returnTypes = resolve(context);
                        return returnTypes
                            .ApplyGraphQlArguments(context);
                    })
            };
            return field;
        }
    }
}