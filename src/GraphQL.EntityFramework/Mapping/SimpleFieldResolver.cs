class SimpleFieldResolver<TSource>(Func<TSource, object?> func) :
    IFieldResolver
{
    public ValueTask<object?> ResolveAsync(IResolveFieldContext context)
    {
        var source = (TSource) context.Source!;
        return ValueTask.FromResult(func(source));
    }
}