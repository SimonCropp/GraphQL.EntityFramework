using System;
using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;

static class Scalars
{
    private static Dictionary<Type, ScalarGraphType>? entries;
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

    public static void RegisterInContainer(IServiceCollection services)
    {
        Initialize();
        foreach (var entry in entries!)
        {
            services.AddSingleton(entry.Key, entry.Value);
        }
    }

    static void Add<T>(Type type)
        where T : ScalarGraphType, new()
    {
        if (GraphTypeTypeRegistry.Get(type) == null)
        {
            GraphTypeTypeRegistry.Register(type, typeof(T));
            var value = new T();
            entries!.Add(typeof(T), value);
        }
    }
}