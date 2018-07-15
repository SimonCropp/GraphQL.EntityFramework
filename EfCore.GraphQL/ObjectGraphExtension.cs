using System.Collections.Generic;
using System.Linq;
using GraphQL.Types.Relay.DataObjects;

namespace EfCoreGraphQL
{
    public static partial class ObjectGraphExtension
    {
        static List<Edge<TReturnType>> BuildEdges<TReturnType>(IEnumerable<TReturnType> result, int skip)
            where TReturnType : class
        {
            return result
                .Select((item, index) =>
                    new Edge<TReturnType>
                    {
                        Cursor = (index + skip).ToString(),
                        Node = item
                    })
                .ToList();
        }
    }
}