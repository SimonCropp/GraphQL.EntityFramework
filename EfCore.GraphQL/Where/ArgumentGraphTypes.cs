using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace EfCoreGraphQL
{
    public static class ArgumentGraphTypes
    {
        static Dictionary<Type, IInputObjectGraphType> entries = new Dictionary<Type, IInputObjectGraphType>();

        static ArgumentGraphTypes()
        {
            Add<WhereExpressionGraphType>();
        }

        static void Add<T>()
            where T : IInputObjectGraphType, new()
        {
            var value = new T();
            entries.Add(typeof(T), value);
        }

        public static bool TryGet(Type type, out IInputObjectGraphType instance)
        {
            if (entries.TryGetValue(type, out instance))
            {
                return true;
            }

            return false;
        }
    }
}