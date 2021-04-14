using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace GraphQL.EntityFramework
{
    public static class NullableTypes
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
                if (CheckForNullable(member.GetCustomAttributes(), out var nullable))
                {
                    return (bool)nullable;
                }
                if (CheckForNullable(member.GetGetMethod()!.GetCustomAttributes(), out nullable))
                {
                    return (bool)nullable;
                }

                if (CheckForNullable(member.DeclaringType!.GetCustomAttributes(), out nullable))
                {
                    return (bool)nullable;
                }

                //Do not return false here due to this causes interpretation of nullable string in #nullable disable case as non nullable one.
                //return false;
            }

            return propertyType.Nullable();
        }

        static bool CheckForNullable(IEnumerable<Attribute> attributes, [NotNullWhen(true)]out bool? isNullable)
        {
            foreach (var attribute in attributes)
            {
                if (NullableFlag(attribute, out isNullable))
                {
                    return true;
                }
            }

            isNullable = null;
            return false;
        }

        static bool NullableFlag(Attribute attribute, [NotNullWhen(true)] out bool? isNullable)
        {
            var type = attribute.GetType();
            switch (type)
            {
                case {Name: "NullableAttribute"}:
                {
                    isNullable = GetNullableFlag(type, attribute);
                    return true;
                }
                case {Name: "NullableContextAttribute"}:
                {
                    isNullable = IsNullableContextAttributeFlagNull(type, attribute);
                    return true;
                }
            }

            isNullable = null;
            return false;
        }

        public static bool Nullable(this Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            if (CheckForNullable(type.GetCustomAttributes(), out var nullable))
            {
                return (bool)nullable;
            }

            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            return false;
        }

        static bool IsNullableContextAttributeFlagNull(Type type, Attribute attribute)
        {
            var field = type.GetField("Flag")!;
            var defaultFlag = (byte) field.GetValue(attribute)!;
            return defaultFlag == 2;
        }
    }
}