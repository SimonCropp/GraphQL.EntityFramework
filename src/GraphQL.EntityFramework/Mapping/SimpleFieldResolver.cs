using GraphQL;
using GraphQL.Resolvers;

class SimpleFieldResolver<TSource> :
    IFieldResolver
{
    Func<TSource, object?> func;

    public SimpleFieldResolver(Func<TSource, object?> func) =>
        this.func = func;

    public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
    {
        var source = (TSource) context.Source!;
        return ValueTask.FromResult(func(source));
    }
}