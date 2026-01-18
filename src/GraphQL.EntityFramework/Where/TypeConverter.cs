static class TypeConverter
{
    static FrozenDictionary<Type, Func<IEnumerable<string>, IList>> listConverters =
        FrozenDictionary.Create<Type, Func<IEnumerable<string>, IList>>(
        [
            new(typeof(Guid), values => values.Select(Guid.Parse).ToList()),
            new(typeof(Guid?), values => values.Select(_ => (Guid?)new Guid(_)).ToList()),
            new(typeof(bool), values => values.Select(ParseBoolean).ToList()),
            new(typeof(bool?), values => values.Select(_ => (bool?)ParseBoolean(_)).ToList()),
            new(typeof(int), values => values.Select(int.Parse).ToList()),
            new(typeof(int?), values => values.Select(_ => (int?)int.Parse(_)).ToList()),
            new(typeof(short), values => values.Select(short.Parse).ToList()),
            new(typeof(short?), values => values.Select(_ => (short?)short.Parse(_)).ToList()),
            new(typeof(long), values => values.Select(long.Parse).ToList()),
            new(typeof(long?), values => values.Select(_ => (long?)long.Parse(_)).ToList()),
            new(typeof(uint), values => values.Select(uint.Parse).ToList()),
            new(typeof(uint?), values => values.Select(_ => (uint?)uint.Parse(_)).ToList()),
            new(typeof(ushort), values => values.Select(ushort.Parse).ToList()),
            new(typeof(ushort?), values => values.Select(_ => (ushort?)ushort.Parse(_)).ToList()),
            new(typeof(ulong), values => values.Select(ulong.Parse).ToList()),
            new(typeof(ulong?), values => values.Select(_ => (ulong?)ulong.Parse(_)).ToList()),
            new(typeof(DateTime), values => values.Select(DateTime.Parse).ToList()),
            new(typeof(DateTime?), values => values.Select(_ => (DateTime?)DateTime.Parse(_)).ToList()),
            new(typeof(Time), values => values.Select(Time.Parse).ToList()),
            new(typeof(Time?), values => values.Select(_ => (Time?)Time.Parse(_)).ToList()),
            new(typeof(Date), values => values.Select(_ => Date.ParseExact(_, "yyyy-MM-dd")).ToList()),
            new(typeof(Date?), values => values.Select(_ => (Date?)Date.ParseExact(_, "yyyy-MM-dd")).ToList()),
            new(typeof(DateTimeOffset), values => values.Select(DateTimeOffset.Parse).ToList()),
            new(typeof(DateTimeOffset?), values => values.Select(_ => (DateTimeOffset?)DateTimeOffset.Parse(_)).ToList()),
        ]);

    static FrozenDictionary<Type, Func<string, object>> singleConverters =
        FrozenDictionary.Create<Type, Func<string, object>>(
        [
            new(typeof(DateTime), value => ValueConverter.ConvertTo<DateTime>(value)),
            new(typeof(Date), value => ValueConverter.ConvertTo<Date>(value)),
            new(typeof(Time), value => ValueConverter.ConvertTo<Time>(value)),
            new(typeof(DateTimeOffset), value => ValueConverter.ConvertTo<DateTimeOffset>(value)),
            new(typeof(Guid), value => new Guid(value)),
        ]);

    public static IList ConvertStringsToList(string?[] values, MemberInfo property)
    {
        var hash = new HashSet<string?>();
        var duplicates = values.Where(_ => !hash.Add(_)).ToArray();
        if (duplicates.Length != 0)
        {
            throw new(
                $"""
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

    static IList ConvertStringsToListInternal(IEnumerable<string> values, Type type)
    {
        // Try dictionary lookup first for common types
        if (listConverters.TryGetValue(type, out var converter))
        {
            return converter(values);
        }

        // Handle enums
        if (type.IsEnum)
        {
            var getList = enumListMethod.MakeGenericMethod(type);
            return (IList)getList.Invoke(null, [values])!;
        }

        if (type.TryGetEnumType(out var enumType))
        {
            var getList = nullableEnumListMethod.MakeGenericMethod(enumType);
            return (IList)getList.Invoke(null, [values])!;
        }

        throw new($"Could not convert strings to {type.FullName}.");
    }

    static MethodInfo enumListMethod = typeof(TypeConverter)
        .GetMethod("GetEnumList", BindingFlags.Static | BindingFlags.NonPublic)!;

    // Use via reflection
    // ReSharper disable once UnusedMember.Local
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

    // Use via reflection
    // ReSharper disable once UnusedMember.Local
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

        // Try dictionary lookup first for common types
        if (singleConverters.TryGetValue(type, out var converter))
        {
            return converter(value!);
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, value!, true);
        }

        return Convert.ChangeType(value, type);
    }
}