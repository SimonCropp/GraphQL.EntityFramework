using System;
using System.Collections.Generic;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TSourceType >: ObjectGraphType<TSourceType>
    {
        protected ConnectionBuilder<TGraphType, TSourceType> AddEnumerableConnectionField<TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName) where TReturnType : class where TGraphType : ObjectGraphType<TReturnType>
        {
            return this.AddEnumerableConnectionField<TSourceType, TGraphType, TReturnType>(name, resolve, includeName);
        }

        protected FieldType AddEnumerableField<TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName = null)
            where TGraphType : ObjectGraphType<TReturnType>
            where TReturnType : class
        {
            return this.AddEnumerableField<TSourceType,TGraphType, TReturnType>(name, resolve, includeName);
        }
    }
}