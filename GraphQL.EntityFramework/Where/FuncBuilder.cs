using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.EntityFramework;

static class FuncBuilder<T>
{
    static ConcurrentDictionary<string, PropertyAccessor> funcs = new ConcurrentDictionary<string, PropertyAccessor>();

    public static Func<T, bool> BuildPredicate(WhereExpression where)
    {
        return BuildPredicate(where.Path, where.Comparison, where.Value,where.Case);
    }

    public static Func<T, bool> BuildPredicate(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
    {
        var propertyFunc = GetPropertyFunc(path);

        if (propertyFunc.Type == typeof(string))
        {
            WhereValidator.ValidateString(comparison, stringComparison);
            var stringComparisonValue = stringComparison.GetValueOrDefault(StringComparison.OrdinalIgnoreCase);
            if (comparison == Comparison.In)
            {
                return BuildStringIn(propertyFunc, values, stringComparisonValue);
            }
            else
            {
                var value = values?.Single();
                return target => BuildStringCompare(comparison, value, propertyFunc, target, stringComparisonValue);
            }
        }
        else
        {
            WhereValidator.ValidateObject(propertyFunc.Type, comparison, stringComparison);
            if (comparison == Comparison.In)
            {
                return BuildObjectIn(propertyFunc, values);
            }
            else
            {
                var value = values?.Single();
                return target => BuildObjectCompare(comparison, value, propertyFunc, target);
            }
        }
    }

    static bool BuildObjectCompare(Comparison comparison, string value, PropertyAccessor propertyFunc, T target)
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
        }
        throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
    }

    static bool BuildStringCompare(Comparison comparison, string value, PropertyAccessor propertyFunc, T x, StringComparison stringComparison)
    {
        var propertyValue = (string) propertyFunc.Func(x);
        switch (comparison)
        {
            case Comparison.Equal:
                return string.Equals(propertyValue, value, stringComparison);
            case Comparison.NotEqual:
                return !string.Equals(propertyValue, value, stringComparison);
            case Comparison.Contains:
                return propertyValue?.IndexOf(value, stringComparison) >= 0;
            case Comparison.StartsWith:
                return propertyValue != null && propertyValue.StartsWith(value, stringComparison);
            case Comparison.EndsWith:
                return propertyValue != null && propertyValue.EndsWith(value, stringComparison);
        }
        throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
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

    static Func<T, bool> BuildObjectIn(PropertyAccessor property, IEnumerable<string> values)
    {
        return target =>
        {
            var propertyValue = property.Func(target);
            return values.Select(x => TypeConverter.ConvertStringToType(x, property.Type))
                .Any(x => x == propertyValue);
        };
    }

    static Func<T, bool> BuildStringIn(PropertyAccessor property, string[] values, StringComparison stringComparison)
    {
        return target =>
        {
            var propertyValue = (string)property.Func(target);
            return values.Any(x => string.Equals(x, propertyValue, stringComparison));
        };
    }

    class PropertyAccessor
    {
        public Func<T, object> Func;
        public Type Type;
    }
}