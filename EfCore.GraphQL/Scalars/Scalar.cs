using System;
using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Utilities;

namespace EfCoreGraphQL
{
    public static class Scalar
    {
        static Dictionary<Type, ScalarGraphType> entries = new Dictionary<Type, ScalarGraphType>();

        public static void Inject(Action<Type,ScalarGraphType> registerInstance = null)
        {
            GraphTypeTypeRegistry.Register(typeof(Guid), typeof(GuidGraphType));
            GraphTypeTypeRegistry.Register(typeof(ulong), typeof(UlongGraphType));
            Add<GuidGraphType>(registerInstance);
            Add<UlongGraphType>(registerInstance);
        }

        static void Add<T>(Action<Type, ScalarGraphType> registerInstance)
            where T : ScalarGraphType, new()
        {
            var value = new T();
            registerInstance?.Invoke(typeof(T), value);
            entries.Add(typeof(T), value);
        }

        public static ScalarGraphType Build(Type type)
        {
            if (entries.TryGetValue(type, out var entry))
            {
                return entry;
            }

            throw new Exception($"Could not find ScalarGraphType: {type.FullName}");
        }
    }
}