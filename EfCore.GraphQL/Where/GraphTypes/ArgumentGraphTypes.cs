using System;
using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Utilities;

namespace EfCoreGraphQL
{
    public static class ArgumentGraphTypes
    {
        static Dictionary<Type, GraphType> entries = new Dictionary<Type, GraphType>();

        static ArgumentGraphTypes()
        {
            GraphTypeTypeRegistry.Register(typeof(Comparison), typeof(ComparisonGraph));
            Add<WhereExpressionGraph>();
            Add<ComparisonGraph>();
        }

        public static void Inject(Action<Type, GraphType> registerInstance)
        {
            foreach (var entry in entries)
            {
                registerInstance(entry.Key, entry.Value);
            }
        }
        static void Add<T>()
            where T : GraphType, new()
        {
            var value = new T();
            entries.Add(typeof(T), value);
        }

        public static bool TryGet(Type type, out GraphType instance)
        {
            if (entries.TryGetValue(type, out instance))
            {
                return true;
            }

            return false;
        }
    }
}