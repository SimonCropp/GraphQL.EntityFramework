static class TypeConverter
{
    public static List<object?> ConvertStringsToList(string?[] values, MemberInfo property)
    {
        var hash = new HashSet<string?>();
        var duplicates = values.Where(_ => !hash.Add(_)).ToArray();
        if (duplicates.Length != 0)
        {
            throw new($"""
                       Duplicates detected for In expression. Duplicates:
                       {string.Join(" * ", duplicates)}
                       """);
        }

        var hasNull = values.Contains(null);

        var type = property.GetNullabilityInfo().Type;
        if (!property.IsNullable() && hasNull)
        {
            throw new($"Null passed to In expression for non nullable type '{type.FullName}'.");
        }

        var list = ConvertStringsToListInternal(values.Where(_ => _ is not null).Select(_ => _!), type);
        if (hasNull)
        {
            list.Add(null);
        }

        return list;
    }

    static bool ParseBoolean(string value) =>
        value switch
        {
            "1" => true,
            "0" => false,
            _ => bool.Parse(value)
        };

    static List<object?> ConvertStringsToListInternal(IEnumerable<string> values, Type type)
    {
        if (type == typeof(Guid))
        {
            return values.Select(s => (object?)Guid.Parse(s)).ToList();
        }
        if (type == typeof(Guid?))
        {
            return values.Select(_ => (object?)new Guid(_)).ToList();
        }

        if (type == typeof(bool))
        {
            return values.Select(s => (object?)ParseBoolean(s)).ToList();
        }
        if (type == typeof(bool?))
        {
            return values.Select(_ => (object?)ParseBoolean(_)).ToList();
        }

        if (type == typeof(int))
        {
            return values.Select(s => (object?)int.Parse(s)).ToList();
        }
        if (type == typeof(int?))
        {
            return values.Select(_ => (object?)int.Parse(_)).ToList();
        }

        if (type == typeof(short))
        {
            return values.Select(s => (object?)short.Parse(s)).ToList();
        }
        if (type == typeof(short?))
        {
            return values.Select(_ => (object?)short.Parse(_)).ToList();
        }

        if (type == typeof(long))
        {
            return values.Select(s => (object?)long.Parse(s)).ToList();
        }
        if (type == typeof(long?))
        {
            return values.Select(_ => (object?)long.Parse(_)).ToList();
        }

        if (type == typeof(uint))
        {
            return values.Select(s => (object?)uint.Parse(s)).ToList();
        }
        if (type == typeof(uint?))
        {
            return values.Select(_ => (object?)uint.Parse(_)).ToList();
        }

        if (type == typeof(ushort))
        {
            return values.Select(s =>(object?) ushort.Parse(s)).ToList();
        }
        if (type == typeof(ushort?))
        {
            return values.Select(_ => (object?)ushort.Parse(_)).ToList();
        }

        if (type == typeof(ulong))
        {
            return values.Select(s => (object?)ulong.Parse(s)).ToList();
        }
        if (type == typeof(ulong?))
        {
            return values.Select(_ => (object?)ulong.Parse(_)).ToList();
        }

        if (type == typeof(DateTime))
        {
            return values.Select(s => (object?)DateTime.Parse(s)).ToList();
        }
        if (type == typeof(DateTime?))
        {
            return values.Select(_ => (object?)DateTime.Parse(_)).ToList();
        }

        if (type == typeof(Time))
        {
            return values.Select(s => (object?)Time.Parse(s)).ToList();
        }
        if (type == typeof(Time?))
        {
            return values.Select(_ => (object?)Time.Parse(_)).ToList();
        }

        if (type == typeof(Date))
        {
            return values.Select(_ => (object?)Date.ParseExact(_, "yyyy-MM-dd")).ToList();
        }
        if (type == typeof(Date?))
        {
            return values.Select(_ => (object?)Date.ParseExact(_, "yyyy-MM-dd")).ToList();
        }

        if (type == typeof(DateTimeOffset))
        {
            return values.Select(s => (object?)DateTimeOffset.Parse(s)).ToList();
        }
        if (type == typeof(DateTimeOffset?))
        {
            return values.Select(_ => (object?)DateTimeOffset.Parse(_)).ToList();
        }

        if (type.IsEnum)
        {
            var getList = enumListMethod.MakeGenericMethod(type);
            return (List<object?>)getList.Invoke(null, [values])!;
        }

        if (type.TryGetEnumType(out var enumType))
        {
            var getList = nullableEnumListMethod.MakeGenericMethod(enumType);
            return  (List<object?>)getList.Invoke(null, [values])!;
        }

        throw new($"Could not convert strings to {type.FullName}.");
    }

    static MethodInfo enumListMethod = typeof(TypeConverter)
        .GetMethod("GetEnumList", BindingFlags.Static | BindingFlags.NonPublic)!;
    static List<T> GetEnumList<T>(IEnumerable<string> values)
        where T : struct
    {
        var list = new List<T>();
        foreach (var value in values)
        {
            list.Add(Enum.Parse<T>(value, true));
        }

        return list;
    }

    static MethodInfo nullableEnumListMethod = typeof(TypeConverter)
        .GetMethod("GetNullableEnumList", BindingFlags.Static | BindingFlags.NonPublic)!;
    static List<T?> GetNullableEnumList<T>(IEnumerable<string> values)
        where T : struct
    {
        var list = new List<T?>();
        foreach (var value in values)
        {
            list.Add(Enum.Parse<T>(value, true));
        }

        return list;
    }

    public static object? ConvertStringToType(string? value, Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType is not null)
        {
            if (value is null)
            {
                return null;
            }

            type = underlyingType;
        }

        if (type == typeof(DateTime))
        {
            return ValueConverter.ConvertTo<DateTime>(value);
        }

        if (type == typeof(Date))
        {
            return ValueConverter.ConvertTo<Date>(value);
        }

        if (type == typeof(Time))
        {
            return ValueConverter.ConvertTo<Time>(value);
        }

        if (type == typeof(DateTimeOffset))
        {
            return ValueConverter.ConvertTo<DateTimeOffset>(value);
        }

        if (type == typeof(Guid))
        {
            return new Guid(value!);
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, value!, true);
        }

        return Convert.ChangeType(value, type);
    }
}