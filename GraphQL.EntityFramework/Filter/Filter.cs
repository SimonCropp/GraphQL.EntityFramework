using System.Threading;
using System.Threading.Tasks;
using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public delegate bool Filter<TSource, in TReturn>(ResolveFieldContext<TSource> context, TReturn input);
    public delegate bool GlobalFilter<in TReturn>(object userContext, TReturn input);
}