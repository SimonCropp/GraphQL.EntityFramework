using System;
using System.Collections.Generic;
using GraphQL.Builders;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public class EfObjectGraphType<TSourceType> : ObjectGraphType<TSourceType>
    {
        protected ConnectionBuilder<TGraphType, TSourceType> AddListConnectionField<TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName)
            where TGraphType : ObjectGraphType<TReturnType>
            where TReturnType : class
        {
            return this.AddListConnectionField<TSourceType, TGraphType, TReturnType>(name, resolve, includeName);
        }

        protected FieldType AddListField<TGraphType, TReturnType>(
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName = null)
            where TGraphType : ObjectGraphType<TReturnType>
            where TReturnType : class
        {
            return this.AddListField<TSourceType, TGraphType, TReturnType>(name, resolve, includeName);
        }

        protected FieldType AddListField<TReturnType>(
            Type graphType,
            string name,
            Func<ResolveFieldContext<TSourceType>, IEnumerable<TReturnType>> resolve,
            string includeName = null)
            where TReturnType : class
        {
            return ObjectGraphExtension.AddListField(this, graphType, name, resolve, includeName);
        }
    }
}