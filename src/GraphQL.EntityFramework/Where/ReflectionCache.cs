using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

static class ReflectionCache
{
    public static MethodInfo StringAny;
    static MethodInfo guidListContains;
    static MethodInfo guidNullableListContains;
    static MethodInfo intListContains;
    static MethodInfo intNullableListContains;
    static MethodInfo boolListContains;
    static MethodInfo boolNullableListContains;
    static MethodInfo shortListContains;
    static MethodInfo shortNullableListContains;
    static MethodInfo longListContains;
    static MethodInfo longNullableListContains;
    static MethodInfo uintListContains;
    static MethodInfo uintNullableListContains;
    static MethodInfo ushortListContains;
    static MethodInfo ushortNullableListContains;
    static MethodInfo ulongListContains;
    static MethodInfo ulongNullableListContains;
    static MethodInfo dateTimeListContains;
    static MethodInfo dateTimeNullableListContains;
    static MethodInfo dateTimeOffsetListContains;
    static MethodInfo dateTimeOffsetNullableListContains;
    public static MethodInfo StringLike = typeof(DbFunctionsExtensions).GetMethod("Like", new[] {typeof(DbFunctions), typeof(string), typeof(string)});
    public static MethodInfo StringEqualComparison = typeof(string).GetMethod("Equals", new[] {typeof(string), typeof(string), typeof(StringComparison)});
    public static MethodInfo StringEqual = typeof(string).GetMethod("Equals", new[] {typeof(string), typeof(string)});
    public static MethodInfo StringStartsWithComparison = typeof(string).GetMethod("StartsWith", new[] {typeof(string), typeof(StringComparison)});
    public static MethodInfo StringStartsWith = typeof(string).GetMethod("StartsWith", new[] {typeof(string)});
    public static MethodInfo StringIndexOfComparison = typeof(string).GetMethod("IndexOf", new[] {typeof(string), typeof(StringComparison)});
    public static MethodInfo StringIndexOf = typeof(string).GetMethod("IndexOf", new[] {typeof(string)});
    public static MethodInfo StringEndsWithComparison = typeof(string).GetMethod("EndsWith", new[] {typeof(string), typeof(StringComparison)});
    public static MethodInfo StringEndsWith = typeof(string).GetMethod("EndsWith", new[] {typeof(string)});

    static ReflectionCache()
    {
        StringAny = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));
        guidListContains = GetContains<Guid>();
        guidNullableListContains = GetContains<Guid?>();
        boolListContains = GetContains<bool>();
        boolNullableListContains = GetContains<bool?>();
        intListContains = GetContains<int>();
        intNullableListContains = GetContains<int?>();
        shortListContains = GetContains<short>();
        shortNullableListContains = GetContains<short?>();
        longListContains = GetContains<long>();
        longNullableListContains = GetContains<long?>();
        uintListContains = GetContains<uint>();
        uintNullableListContains = GetContains<uint?>();
        ushortListContains = GetContains<ushort>();
        ushortNullableListContains = GetContains<ushort?>();
        ulongListContains = GetContains<ulong>();
        ulongNullableListContains = GetContains<ulong?>();
        dateTimeListContains = GetContains<DateTime>();
        dateTimeNullableListContains = GetContains<DateTime?>();
        dateTimeOffsetListContains = GetContains<DateTimeOffset>();
        dateTimeOffsetNullableListContains = GetContains<DateTimeOffset?>();
    }

    public static MethodInfo? GetListContains(Type type)
    {
        if (type == typeof(Guid))
        {
            return guidListContains;
        }

        if (type == typeof(Guid?))
        {
            return guidNullableListContains;
        }

        if (type == typeof(bool))
        {
            return boolListContains;
        }

        if (type == typeof(bool?))
        {
            return boolNullableListContains;
        }

        if (type == typeof(int))
        {
            return intListContains;
        }

        if (type == typeof(int?))
        {
            return intNullableListContains;
        }

        if (type == typeof(short))
        {
            return shortListContains;
        }

        if (type == typeof(short?))
        {
            return shortNullableListContains;
        }

        if (type == typeof(long))
        {
            return longListContains;
        }

        if (type == typeof(long?))
        {
            return longNullableListContains;
        }

        if (type == typeof(uint))
        {
            return uintListContains;
        }

        if (type == typeof(uint?))
        {
            return uintNullableListContains;
        }

        if (type == typeof(ushort))
        {
            return ushortListContains;
        }

        if (type == typeof(ushort?))
        {
            return ushortNullableListContains;
        }

        if (type == typeof(ulong))
        {
            return ulongListContains;
        }

        if (type == typeof(ulong?))
        {
            return ulongNullableListContains;
        }

        if (type == typeof(DateTime))
        {
            return dateTimeListContains;
        }

        if (type == typeof(DateTime?))
        {
            return dateTimeNullableListContains;
        }

        if (type == typeof(DateTimeOffset))
        {
            return dateTimeOffsetListContains;
        }

        if (type == typeof(DateTimeOffset?))
        {
            return dateTimeOffsetNullableListContains;
        }

        return null;
    }

    static MethodInfo GetContains<T>()
    {
        return typeof(List<T>).GetMethod("Contains");
    }

    public static bool TryGetEnumType(this Type type, [NotNullWhen(true)] out Type? enumType)
    {
        if (type.IsEnum)
        {
            enumType = type;
            return true;
        }

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying == null)
        {
            enumType = typeof(object);
            return false;
        }

        if (underlying.IsEnum)
        {
            enumType = underlying;
            return true;
        }

        enumType = null;
        return false;
    }

    public static IEnumerable<PropertyInfo> GetPublicProperties(this Type type)
    {
        var flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance;
        if (!type.IsInterface)
        {
            return type.GetProperties(flags);
        }

        var propertyInfos = new List<PropertyInfo>();

        var considered = new List<Type>();
        var queue = new Queue<Type>();
        considered.Add(type);
        queue.Enqueue(type);
        while (queue.Count > 0)
        {
            var subType = queue.Dequeue();
            foreach (var subInterface in subType.GetInterfaces())
            {
                if (considered.Contains(subInterface))
                {
                    continue;
                }

                considered.Add(subInterface);
                queue.Enqueue(subInterface);
            }

            var typeProperties = subType.GetProperties(flags);

            var newPropertyInfos = typeProperties
                .Where(x => !propertyInfos.Contains(x));

            propertyInfos.InsertRange(0, newPropertyInfos);
        }

        return propertyInfos.ToArray();
    }


    public static bool TryGetCollectionType(
        this Type type,
        [NotNullWhen(true)] out Type? itemType)
    {
        itemType = type.GetInterfaces()
            .FirstOrDefault(x =>
            {
                if (!x.IsGenericType)
                {
                    return false;
                }

                var genericType = x.GetGenericTypeDefinition();
                return genericType == typeof(ICollection<>) ||
                       genericType == typeof(IReadOnlyCollection<>);
            });
        return itemType != null;
    }
}