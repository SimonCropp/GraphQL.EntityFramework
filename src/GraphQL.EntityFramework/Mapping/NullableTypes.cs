using System;
using System.Linq;
using System.Reflection;

namespace GraphQL.EntityFramework
{
    static class NullableTypes
    {
        static bool GetNullableFlag(Type type, Attribute attribute)
        {
            var field = type.GetField("NullableFlags")!;
            var nullableFlags = (byte[]) field.GetValue(attribute)!;
            return nullableFlags.Single() == 2;
        }

        public static bool IsNullable(this PropertyInfo member)
        {
            var propertyType = member.PropertyType;
            if (!propertyType.IsValueType)
            {
                foreach (var attribute in member.GetCustomAttributes())
                {
                    var type = attribute.GetType();
                    if (type.Name == "NullableAttribute")
                    {
                        return GetNullableFlag(type, attribute);
                    }
                }

                foreach (var attribute in member.GetGetMethod()!.GetCustomAttributes())
                {
                    var type = attribute.GetType();
                    switch (type.Name)
                    {
                        case "NullableAttribute":
                            return GetNullableFlag(type, attribute);
                        case "NullableContextAttribute":
                            return IsNullableContextAttributeFlagNull(type, attribute);
                    }
                }

                foreach (var attribute in member.DeclaringType!.GetCustomAttributes())
                {
                    var type = attribute.GetType();
                    if (type.Name == "NullableContextAttribute")
                    {
                        return IsNullableContextAttributeFlagNull(type, attribute);
                    }
                    if (type.Name == "NullableAttribute")
                    {
                        return GetNullableFlag(type, attribute);
                    }
                }

                //Do not return false here due to this causes interpretation of nullable string in #nullable disable case as non nullable one.
                //return false;
            }

            return propertyType.IsNullable();
        }

        static bool IsNullableContextAttributeFlagNull(Type type, Attribute attribute)
        {
            var field = type.GetField("Flag")!;
            var defaultFlag = (byte) field.GetValue(attribute)!;
            return defaultFlag == 2;
        }
    }
}