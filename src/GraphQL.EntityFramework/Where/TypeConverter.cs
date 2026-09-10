static class TypeConverter
{
    static CultureInfo invariant = CultureInfo.InvariantCulture;

    // Every parse is invariant. Where values are query text, so a server running under de-DE must
    // read "1.5" the same way as one running under en-US. The date and time types go through
    // GraphQL.NET's converters so a single comparison and an `in` list accept the same literals.
    static FrozenDictionary<Type, Func<string, object>> converters =
        FrozenDictionary.Create<Type, Func<string, object>>(
        [
            new(typeof(Guid), _ => Guid.Parse(_)),
            new(typeof(bool), _ => ParseBoolean(_)),
            new(typeof(byte), _ => byte.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(sbyte), _ => sbyte.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(short), _ => short.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(ushort), _ => ushort.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(int), _ => int.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(uint), _ => uint.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(long), _ => long.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(ulong), _ => ulong.Parse(_, NumberStyles.Integer, invariant)),
            new(typeof(float), _ => float.Parse(_, NumberStyles.Float | NumberStyles.AllowThousands, invariant)),
            new(typeof(double), _ => double.Parse(_, NumberStyles.Float | NumberStyles.AllowThousands, invariant)),
            new(typeof(decimal), _ => decimal.Parse(_, NumberStyles.Number, invariant)),
            new(typeof(DateTime), _ => ValueConverter.ConvertTo<DateTime>(_)),
            new(typeof(Date), _ => ValueConverter.ConvertTo<Date>(_)),
            new(typeof(Time), _ => ValueConverter.ConvertTo<Time>(_)),
            new(typeof(DateTimeOffset), _ => ValueConverter.ConvertTo<DateTimeOffset>(_)),
        ]);

    public static IList ConvertToList(object?[] values, MemberInfo property)
    {
        var type = property.GetNullabilityInfo().Type;
        var converted = values
            .Select(_ => ConvertValue(_, type))
            .ToList();

        var hash = new HashSet<object?>();
        var duplicates = converted.Where(_ => !hash.Add(_)).ToArray();
        if (duplicates.Length != 0)
        {
            throw new(
                $"""
                 Duplicates detected for In expression. Duplicates:
                 {string.Join(" * ", duplicates.Select(_ => Convert.ToString(_, invariant)))}
                 """);
        }

        var hasNull = converted.Contains(null);
        if (!property.IsNullable() && hasNull)
        {
            throw new($"Null passed to In expression for non nullable type '{type.FullName}'.");
        }

        // The list is of the member type, nullable included, since the Contains resolved for the
        // comparison is ICollection<MemberType>.Contains
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;
        foreach (var value in converted)
        {
            if (value is not null)
            {
                list.Add(value);
            }
        }

        if (hasNull)
        {
            list.Add(null);
        }

        return list;
    }

    /// <summary>
    /// A value from the where argument is already of the property type, since the comparison
    /// input type is typed. A string is parsed, for the string based predicate builder and ids.
    /// </summary>
    public static object? ConvertValue(object? value, Type type)
    {
        if (value is string text)
        {
            return ConvertStringToType(text, type);
        }

        if (value is null)
        {
            return ConvertStringToType(null, type);
        }

        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying.IsInstanceOfType(value))
        {
            return value;
        }

        if (underlying.IsEnum)
        {
            return Enum.ToObject(underlying, value);
        }

        return Convert.ChangeType(value, underlying, invariant);
    }

    static bool ParseBoolean(string value) =>
        value switch
        {
            "1" => true,
            "0" => false,
            _ => bool.Parse(value)
        };

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

        if (value is null)
        {
            throw new($"Null passed for non nullable type '{type.FullName}'.");
        }

        if (converters.TryGetValue(type, out var converter))
        {
            return converter(value);
        }

        if (type.IsEnum)
        {
            return Enum.Parse(type, value, true);
        }

        return Convert.ChangeType(value, type, invariant);
    }
}
