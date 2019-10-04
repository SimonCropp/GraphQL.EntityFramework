using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

static class PropertyCache<TInput>
{
    static ConcurrentDictionary<string, Property<TInput>> properties = new ConcurrentDictionary<string, Property<TInput>>();

    public static Property<TInput> GetProperty(string path)
    {
        return properties.GetOrAdd(path,
            x =>
            {
                var parameter = Expression.Parameter(typeof(TInput));
                var left = AggregatePath(x, parameter);

                var converted = Expression.Convert(left, typeof(object));
                var lambda = Expression.Lambda<Func<TInput, object>>(converted, parameter);
                var compile = lambda.Compile();
                var listContainsMethod = ReflectionCache.GetListContains(left.Type);
                return new Property<TInput>
                (
                    left: left,
                    lambda: lambda,
                    sourceParameter: parameter,
                    func: compile,
                    propertyType: left.Type,
                    listContainsMethod: listContainsMethod
                );
            });
    }

    static Expression AggregatePath(string path, Expression parameter)
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