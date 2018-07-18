using System;
using System.Linq;
using GraphQL;

static class TypeConverter
{
    public static object ConvertStringsToList(string[] values, Type type)
    {
        if (type == typeof(Guid))
        {
            return values.Select(Guid.Parse).ToList();
        }
        //todo: implement other type to prevent boxing on contains expression

        return values.Select(x => ConvertStringToType(x, type)).ToList();
    }

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