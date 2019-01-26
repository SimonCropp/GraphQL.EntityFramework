using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.EntityFramework;

static class FuncBuilder<T>
{
    static ConcurrentDictionary<string, PropertyAccessor> funcs = new ConcurrentDictionary<string, PropertyAccessor>();

    public static Func<T, bool> BuildPredicate(WhereExpression where)
    {
        return BuildPredicate(where.Path, where.Comparison.GetValueOrDefault(), where.Value, where.Case);
    }

    public static Func<T, object> BuildPropertyExpression(string path)
    {
        var propertyFunc = GetPropertyFunc(path);
        return propertyFunc.Func;
    }

    public static Func<T, bool> BuildPredicate(string path, Comparison comparison, string[] values, StringComparison? stringComparison = null)
    {
        var propertyFunc = GetPropertyFunc(path);

        if (propertyFunc.Type == typeof(string))
        {
            WhereValidator.ValidateString(comparison, stringComparison);
            var stringComparisonValue = stringComparison.GetValueOrDefault(StringComparison.OrdinalIgnoreCase);
            switch (comparison)
            {
                case Comparison.In:
                    return BuildStringIn(propertyFunc, values, stringComparisonValue);

                case Comparison.NotIn:
                    return BuildStringIn(propertyFunc, values, stringComparisonValue, true);

                default:
                    var value = values?.Single();
                    return target => BuildStringCompare(comparison, value, propertyFunc, target, stringComparisonValue);
            }
        }
        else
        {
            WhereValidator.ValidateObject(propertyFunc.Type, comparison, stringComparison);
            switch (comparison)
            {
                case Comparison.In:
                    return BuildObjectIn(propertyFunc, values);

                case Comparison.NotIn:
                    return BuildObjectIn(propertyFunc, values, true);

                default:
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
            var left = PropertyAccessorBuilder<T>.AggregatePath(x, parameter);

            var converted = Expression.Convert(left, typeof(object));
            var compile = Expression.Lambda<Func<T, object>>(converted, parameter).Compile();

            return new PropertyAccessor
            {
                Func = compile,
                Type = left.Type
            };
        });
    }

    static Func<T, bool> BuildObjectIn(PropertyAccessor property, string[] values, bool not = false)
    {
        return target =>
        {
            var type = property.Type;
            var propertyValue = property.Func(target);

            if (type == typeof(Guid))
            {
                var value = (Guid)propertyValue;
                var result = values.Select(Guid.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(int))
            {
                var value = (int)propertyValue;
                var result = values.Select(int.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(short))
            {
                var value = (short)propertyValue;
                var result = values.Select(short.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(long))
            {
                var value = (long)propertyValue;
                var result = values.Select(long.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(uint))
            {
                var value = (uint)propertyValue;
                var result = values.Select(uint.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(ushort))
            {
                var value = (ushort)propertyValue;
                var result = values.Select(ushort.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(ulong))
            {
                var value = (ulong)propertyValue;
                var result = values.Select(ulong.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(DateTime))
            {
                var value = (DateTime)propertyValue;
                var result = values.Select(DateTime.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            if (type == typeof(DateTimeOffset))
            {
                var value = (DateTimeOffset)propertyValue;
                var result = values.Select(DateTimeOffset.Parse)
                    .Any(x => x == value);
                return not ? !result : result;
            }

            throw new Exception($"Could not convert strings to {type.FullName} ");
        };
    }

    static Func<T, bool> BuildStringIn(PropertyAccessor property, string[] values, StringComparison stringComparison, bool not = false)
    {
        return target =>
        {
            var propertyValue = (string)property.Func(target);
            var result = values.Any(x => string.Equals(x, propertyValue, stringComparison));
            return not ? !result : result;
        };
    }

    class PropertyAccessor
    {
        public Func<T, object> Func;
        public Type Type;
    }
}