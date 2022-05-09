using GraphQL;

static class TypeConverter
{
    public static IList ConvertStringsToList(string?[] values, MemberInfo property)
    {
        if (values.Length != values.Distinct().Count())
        {
            throw new("Duplicates detected for In expression.");
        }

        var hasNull = values.Contains(null);

        var type = property.GetNullabilityInfo().Type;
        if (!property.IsNullable() && hasNull)
        {
            throw new($"Null passed to In expression for non nullable type '{type.FullName}'.");
        }

        var list = ConvertStringsToListInternal(values.Where(x => x is not null).Select(x => x!), type);
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

    static IList ConvertStringsToListInternal(IEnumerable<string> values, Type type)
    {
        if (type == typeof(Guid))
        {
            return values.Select(Guid.Parse).ToList();
        }
        if (type == typeof(Guid?))
        {
            return values.Select(s => new Guid?(Guid.Parse(s))).ToList();
        }

        if (type == typeof(bool))
        {
            return values.Select(ParseBoolean).ToList();
        }
        if (type == typeof(bool?))
        {
            return values.Select(s => new bool?(ParseBoolean(s))).ToList();
        }

        if (type == typeof(int))
        {
            return values.Select(int.Parse).ToList();
        }
        if (type == typeof(int?))
        {
            return values.Select(s => new int?(int.Parse(s))).ToList();
        }

        if (type == typeof(short))
        {
            return values.Select(short.Parse).ToList();
        }
        if (type == typeof(short?))
        {
            return values.Select(s => new short?(short.Parse(s))).ToList();
        }

        if (type == typeof(long))
        {
            return values.Select(long.Parse).ToList();
        }
        if (type == typeof(long?))
        {
            return values.Select(s => new long?(long.Parse(s))).ToList();
        }

        if (type == typeof(uint))
        {
            return values.Select(uint.Parse).ToList();
        }
        if (type == typeof(uint?))
        {
            return values.Select(s => new uint?(uint.Parse(s))).ToList();
        }

        if (type == typeof(ushort))
        {
            return values.Select(ushort.Parse).ToList();
        }
        if (type == typeof(ushort?))
        {
            return values.Select(s => new ushort?(ushort.Parse(s))).ToList();
        }

        if (type == typeof(ulong))
        {
            return values.Select(ulong.Parse).ToList();
        }
        if (type == typeof(ulong?))
        {
            return values.Select(s => new ulong?(ulong.Parse(s))).ToList();
        }

        if (type == typeof(DateTime))
        {
            return values.Select(DateTime.Parse).ToList();
        }
        if (type == typeof(DateTime?))
        {
            return values.Select(s => new DateTime?(DateTime.Parse(s))).ToList();
        }

        if (type == typeof(DateTimeOffset))
        {
            return values.Select(DateTimeOffset.Parse).ToList();
        }
        if (type == typeof(DateTimeOffset?))
        {
            return values.Select(s => new DateTimeOffset?(DateTimeOffset.Parse(s))).ToList();
        }

        if (type.TryGetEnumType(out var enumType))
        {
            return values.Select(s => Enum.Parse(enumType, s, true))
                .ToList();
        }

        throw new($"Could not convert strings to {type.FullName}.");
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

        if (type == typeof(DateTimeOffset))
        {
            return ValueConverter.ConvertTo<DateTimeOffset>(value);
        }

        if (type == typeof(Guid))
        {
            return Guid.Parse(value!);
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, value!, true);
        }

        return Convert.ChangeType(value, type);
    }
}