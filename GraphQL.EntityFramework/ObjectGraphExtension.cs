using System;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }
    }
}