using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

static class PropertyAccessorBuilder<TInput>
{
    static ConcurrentDictionary<string, PropertyFunc<TInput>> propertyFuncs = new ConcurrentDictionary<string, PropertyFunc<TInput>>();

    static ConcurrentDictionary<string, PropertyExpression> funcExpressions = new ConcurrentDictionary<string, PropertyExpression>();

    public static PropertyFunc<TInput> GetFunc(string path)
    {
        return propertyFuncs.GetOrAdd(path, x =>
        {
            var parameter = Expression.Parameter(typeof(TInput));
            var left = AggregatePath(x, parameter);

            var converted = Expression.Convert(left, typeof(object));
            var compile = Expression.Lambda<Func<TInput, object>>(converted, parameter).Compile();

            return new PropertyFunc<TInput>
            {
                Func = compile,
                PropertyType = left.Type
            };
        });
    }
    public static PropertyExpression GetExpression(string path)
    {
        return funcExpressions.GetOrAdd(path, x =>
        {
            var parameter = Expression.Parameter(typeof(TInput));
            var aggregatePath = AggregatePath(x, parameter);
            return new PropertyExpression
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