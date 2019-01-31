using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

static class PropertyAccessorBuilder<TInput>
{
    static ConcurrentDictionary<string, PropertyAccessor> funcs = new ConcurrentDictionary<string, PropertyAccessor>();

    public static PropertyAccessor GetPropertyFunc(string propertyPath)
    {
        return funcs.GetOrAdd(propertyPath, path =>
        {
            var parameter = Expression.Parameter(typeof(TInput));
            var aggregatePath = AggregatePath(path, parameter);
            return new PropertyAccessor
            {
                SourceParameter = parameter,
                Left = aggregatePath,
                PropertyType = aggregatePath.Type
            };
        });
    }

    public static Expression AggregatePath(string path, Expression parameter)
    {
        try
        {
            return path.Split('.')
                .Aggregate(parameter, Expression.PropertyOrField);
        }
        catch (ArgumentException exception)
        {
            throw new Exception($"Failed to create a member expression. Type: {typeof(TInput).FullName}, Path: {path}. Error: {exception.Message}");
        }
    }
}