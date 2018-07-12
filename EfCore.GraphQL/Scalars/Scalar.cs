using System;
using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Utilities;

namespace EfCoreGraphQL
{
    public static class Scalar
    {
        private static readonly Dictionary<Type, ScalarGraphType> entries = new Dictionary<Type, ScalarGraphType>() ;

        public static void Inject()
        {
            GraphTypeTypeRegistry.Register(typeof(Guid), typeof(GuidGraphType));
            GraphTypeTypeRegistry.Register(typeof(ulong), typeof(UlongGraphType));
            Add< GuidGraphType>();
            Add<UlongGraphType>();
        }

        static void Add<T>() where T : ScalarGraphType, new()
        {
            entries.Add(typeof(T), new T());
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