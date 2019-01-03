using System;
using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Utilities;

namespace GraphQL.EntityFramework
{
    public static class Scalars
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
                Add<GuidGraph>(typeof(Guid));
                Add<UlongGraph>(typeof(ulong));
                Add<UintGraph>(typeof(uint));
                Add<UshortGraph>(typeof(ushort));
                Add<ShortGraph>(typeof(short));
            }
        }

        public static void RegisterInContainer(Action<Type, ScalarGraphType> registerInstance)
        {
            Initialize();
            foreach (var entry in entries)
            {
                registerInstance(entry.Key, entry.Value);
            }
        }

        static void Add<T>(Type type)
            where T : ScalarGraphType, new()
        {
            if (GraphTypeTypeRegistry.Get(type) == null)
            {
                GraphTypeTypeRegistry.Register(type, typeof(T));
                var value = new T();
                entries.Add(typeof(T), value);
            }
        }
    }
}