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
            string includeName = null)
            where TGraphType : ObjectGraphType<TReturnType>, IGraphType
            where TReturnType : class
        {
            var field = BuildEnumerableField<object, TGraphType, TReturnType>(name, resolve, includeName);
            return graphType.AddField(field);
        }

        public static FieldType AddEnumerableField<TSourceType, TGraphType, TReturnType>(
            this ObjectGraphType<TSourceType> graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName = null)
            where TGraphType : ObjectGraphType<TReturnType>, IGraphType
            where TReturnType : class
        {
            var field = BuildEnumerableField<TSourceType, TGraphType, TReturnType>(name, resolve,  includeName);
            return graphType.AddField(field);
        }

        static FieldType BuildEnumerableField<TSourceType, TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>,
            IEnumerable<TReturnType>> resolve,
            string includeName)
            where TGraphType : ObjectGraphType<TReturnType>, IGraphType
            where TReturnType : class
        {
            var field = new FieldType
            {
                Name = name,
                Type = typeof(ListGraphType<TGraphType>),
                Arguments = ArgumentAppender.GetQueryArguments(),
                Resolver = new FuncFieldResolver<TSourceType, IEnumerable<TReturnType>>(
                    context =>
                    {
                        var returnTypes = resolve(context);
                        return returnTypes
                            .ApplyGraphQlArguments(context);
                    })
            };
            field.SetIncludeMetadata(includeName);
            return field;
        }
    }
}