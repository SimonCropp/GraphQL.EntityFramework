using GraphQL.Types;

namespace GraphQL.EntityFramework
{
    public delegate bool Filter<TSource, in TReturn>(ResolveFieldContext<TSource> context, TReturn input);
}