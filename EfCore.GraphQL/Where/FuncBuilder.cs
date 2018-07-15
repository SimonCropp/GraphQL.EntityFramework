using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EfCoreGraphQL;

static class FuncBuilder<T>
{
    public static Func<T, bool> BuildPredicate(WhereExpression whereExpression)
    {
        if (whereExpression.Comparison == Comparison.In)
        {
            return BuildIn(whereExpression.Path, whereExpression.Value);
        }

        return BuildPredicate(whereExpression.Path, whereExpression.Comparison, whereExpression.Value.Single());
    }

    class PropertyAccessor
    {
        public Func<T, object> Func;
        public Type Type;
    }

    static ConcurrentDictionary<string, PropertyAccessor> funcs = new ConcurrentDictionary<string, PropertyAccessor>();

    public static Func<T, bool> BuildIn(string propertyPath, IEnumerable<string> values)
    {
        var propertyFunc = GetPropertyFunc(propertyPath);

        if (propertyFunc.Type == typeof(string))
        {
            return target =>
            {
                var propertyValue = (string) propertyFunc.Func(target);
                return values.Contains(propertyValue);
            };
        }

        return target =>
        {
            var propertyValue = propertyFunc.Func(target);

            return values.Select(x => TypeConverter.ConvertStringToType(x, propertyFunc.Type)).Any(x => x == propertyValue);
        };
    }

    public static Func<T, bool> BuildPredicate(string propertyPath, Comparison comparison, string value)
    {
        var propertyFunc = GetPropertyFunc(propertyPath);

        if (propertyFunc.Type == typeof(string))
        {
            return target => BuildStringCompare(comparison, value, propertyFunc, target);
        }

        return target => BuildCompare(comparison, value, propertyFunc, target);
    }

    static bool BuildCompare(Comparison comparison, string value, PropertyAccessor propertyFunc, T target)
    {
        var propertyValue = propertyFunc.Func(target);
        var typedValue = TypeConverter.ConvertStringToType(value, propertyFunc.Type);

        var compare = Compare(propertyValue, typedValue);
        switch (comparison)
        {
            case Comparison.Equal:
                return compare == 0;
            case Comparison.NotEqual:
                return compare != 0;
            case Comparison.GreaterThan:
                return compare > 0;
            case Comparison.GreaterThanOrEqual:
                return compare >= 0;
            case Comparison.LessThanOrEqual:
                return compare <= 0;
            case Comparison.LessThan:
                return compare < 0;
            case Comparison.Contains:
            case Comparison.StartsWith:
            case Comparison.EndsWith:
                throw new Exception($"Cannot perform {comparison} on {propertyFunc.Type}.");
            default:
                throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
        }
    }

    static bool BuildStringCompare(Comparison comparison, string value, PropertyAccessor propertyFunc, T x)
    {
        var propertyValue = (string) propertyFunc.Func(x);
        switch (comparison)
        {
            case Comparison.Equal:
                return propertyValue == value;
            case Comparison.NotEqual:
                return propertyValue != value;
            case Comparison.Contains:
                return propertyValue.Contains(value);
            case Comparison.StartsWith:
                return propertyValue.StartsWith(value);
            case Comparison.EndsWith:
                return propertyValue.EndsWith(value);
            case Comparison.GreaterThan:
            case Comparison.GreaterThanOrEqual:
            case Comparison.LessThanOrEqual:
            case Comparison.LessThan:
                throw new Exception($"Cannot perform {comparison} on string.");
            default:
                throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null);
        }
    }


    static int Compare(object a, object b)
    {
        if (a == null && b == null)
        {
            return 0;
        }

        if (a == null)
        {
            return -1;
        }

        var ac = (IComparable) a;
        var bc = (IComparable) b;

        return ac.CompareTo(bc);
    }

    static PropertyAccessor GetPropertyFunc(string propertyPath)
    {
        return funcs.GetOrAdd(propertyPath, x =>
        {
            var parameter = Expression.Parameter(typeof(T));
            var left = AggregatePath(x, parameter);

            var converted = Expression.Convert(left, typeof(object));
            var compile = Expression.Lambda<Func<T, object>>(converted, parameter).Compile();

            return new PropertyAccessor
            {
                Func = compile,
                Type = left.Type
            };
        });
    }

    static Expression AggregatePath(string propertyPath, Expression parameter)
    {
        return propertyPath.Split('.')
            .Aggregate(parameter, Expression.PropertyOrField);
    }
}