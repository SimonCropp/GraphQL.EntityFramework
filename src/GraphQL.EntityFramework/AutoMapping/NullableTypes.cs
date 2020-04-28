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
                    if (type.Name == "NullableAttribute")
                    {
                        return GetNullableFlag(type, attribute);
                    }
                }

                foreach (var attribute in member.DeclaringType!.GetCustomAttributes())
                {
                    var type = attribute.GetType();
                    if (type.Name == "NullableContextAttribute")
                    {
                        var field = type.GetField("Flag")!;
                        var defaultFlag = (byte) field.GetValue(attribute)!;
                        return defaultFlag == 2;
                    }
                }
            }

            return propertyType.IsNullable();
        }
    }
}