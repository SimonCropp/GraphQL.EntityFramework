using System;

static class TypeConverter
{
    public static object ConvertStringToType(string value, Type type)
    {
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