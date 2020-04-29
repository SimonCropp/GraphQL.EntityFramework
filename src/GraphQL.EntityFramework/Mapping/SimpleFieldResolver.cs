using System;
using GraphQL.Resolvers;
using GraphQL.Types;

class SimpleFieldResolver :
    IFieldResolver
{
    Func<object, object> func;

    public SimpleFieldResolver(Func<object, object> func)
    {
        this.func = func;
    }

    public object Resolve(ResolveFieldContext context)
    {
        return func(context.Source);
    }
}