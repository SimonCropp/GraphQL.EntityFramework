using System;
using GraphQL.Resolvers;
using GraphQL.Types;

class SimpleFieldResolver<TSource> :
    IFieldResolver
{
    Func<TSource, object> func;

    public SimpleFieldResolver(Func<TSource, object> func)
    {
        this.func = func;
    }

    public object Resolve(ResolveFieldContext context)
    {
        return func((TSource) context.Source);
    }
}