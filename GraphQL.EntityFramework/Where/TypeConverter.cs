using System;
using GraphQL;

static class TypeConverter
{
    public static object ConvertStringToType(string value, Type type)
    {
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

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            if (value == null)
            {
                return null;
            }

            type = underlyingType;
        }

        return Convert.ChangeType(value, type);
    }
}