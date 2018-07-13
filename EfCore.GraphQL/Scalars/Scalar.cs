using System;
using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Utilities;

namespace EfCoreGraphQL
{
    public static class Scalar
    {
        static Dictionary<Type, ScalarGraphType> entries;
        static object locker = new object();

        public static void Initialize()
        {
            if (entries != null)
            {
                return;
            }

            lock (locker)
            {
                if (entries != null)
                {
                    return;
                }

                entries = new Dictionary<Type, ScalarGraphType>();
                GraphTypeTypeRegistry.Register(typeof(Guid), typeof(GuidGraph));
                GraphTypeTypeRegistry.Register(typeof(ulong), typeof(UlongGraph));
                Add<GuidGraph>();
                Add<UlongGraph>();
            }
        }

        public static void Inject(Action<Type, ScalarGraphType> registerInstance)
        {
            Initialize();
            foreach (var entry in entries)
            {
                registerInstance(entry.Key, entry.Value);
            }
        }

        static void Add<T>()
            where T : ScalarGraphType, new()
        {
            var value = new T();
            entries.Add(typeof(T), value);
        }

        public static bool TryGet(Type type, out ScalarGraphType instance)
        {
            if (entries.TryGetValue(type, out instance))
            {
                return true;
            }

            return false;
        }
    }
}