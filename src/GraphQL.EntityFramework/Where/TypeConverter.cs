using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL;

static class TypeConverter
{
    public static IList ConvertStringsToList(string[] values, Type type)
    {
        var distinctValues = new HashSet<string>(values);

        var hasNull = distinctValues.Contains(null);

        var newType = type.IsNullable()
            ? Nullable.GetUnderlyingType(type)
            : type;

        var valuesExceptNull = distinctValues.Where(s => s != null).ToArray();
        var result = ConvertStringsToListInternal(valuesExceptNull, newType);

        return type.IsNullable() ? MakeNullableListFrom(result, type, hasNull) : result;
    }

    private static IList MakeNullableListFrom(IList result, Type newType, bool hasNull = false)
    {
        var listOfNullableValues = MakeListInternal(result, newType);

        if (hasNull)
        {
            listOfNullableValues.Add(null);
        }

        return listOfNullableValues;
    }

    private static IList MakeListInternal(IList input, Type type)
    {
        var method = typeof(Enumerable).GetMethod(nameof(Enumerable.OfType),BindingFlags.Static | BindingFlags.Public)
            ?.MakeGenericMethod(type);
        
        var args = method.Invoke(null, new object[] {input});
        
        var result = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(type),args);

        return result;
    }

    private static IList ConvertStringsToListInternal(string[] values, Type type)
    {
        if (type == typeof(Guid))
        {
            return values.Select(Guid.Parse).ToList();
        }

        if (type == typeof(int))
        {
            return values.Select(int.Parse).ToList();
        }

        if (type == typeof(short))
        {
            return values.Select(short.Parse).ToList();
        }

        if (type == typeof(long))
        {
            return values.Select(long.Parse).ToList();
        }

        if (type == typeof(uint))
        {
            return values.Select(uint.Parse).ToList();
        }

        if (type == typeof(ushort))
        {
            return values.Select(ushort.Parse).ToList();
        }

        if (type == typeof(ulong))
        {
            return values.Select(ulong.Parse).ToList();
        }

        if (type == typeof(DateTime))
        {
            return values.Select(DateTime.Parse).ToList();
        }

        if (type == typeof(DateTimeOffset))
        {
            return values.Select(DateTimeOffset.Parse).ToList();
        }

        throw new Exception($"Could not convert strings to {type.FullName} ");
    }

    public static object ConvertStringToType(string value, Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            if (value == null)
            {
                return null;
            }

            type = underlyingType;
        }

        if (type == typeof(DateTime))
        {
            return ValueConverter.ConvertTo<DateTime>(value);
        }

        if (type == typeof(DateTimeOffset))
        {
            return ValueConverter.ConvertTo<DateTimeOffset>(value);
        }

        if (type == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        return Convert.ChangeType(value, type);
    }
}