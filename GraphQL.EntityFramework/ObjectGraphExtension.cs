using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using GraphQL.Types.Relay.DataObjects;

namespace GraphQL.EntityFramework
{
    public static partial class ObjectGraphExtension
    {
        static List<Edge<TReturn>> BuildEdges<TReturn>(IEnumerable<TReturn> result, int skip)
            where TReturn : class
        {
            return result
                .Select((item, index) =>
                    new Edge<TReturn>
                    {
                        Cursor = (index + skip).ToString(),
                        Node = item
                    })
                .ToList();
        }

        static Type MakeListGraphType(Type graphType)
        {
            var listGraphType = typeof(ListGraphType<>);
            return listGraphType.MakeGenericType(graphType);
        }
    }
}