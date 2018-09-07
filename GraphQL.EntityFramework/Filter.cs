using System.Collections.Generic;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public delegate IEnumerable<TReturn> Filter<TSource,TReturn>(ResolveFieldContext<TSource> context,IEnumerable<TReturn> input);
}