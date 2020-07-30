using System;
using GraphQL;
using GraphQL.Resolvers;

class SimpleFieldResolver<TSource> :
    IFieldResolver
{
    Func<TSource, object> func;

    public SimpleFieldResolver(Func<TSource, object> func)
    {
        this.func = func;
    }

    public object Resolve(IResolveFieldContext context)
    {
        return func((TSource) context.Source);
    }
}