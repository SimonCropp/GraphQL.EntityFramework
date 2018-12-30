using System;
using System.Collections.Generic;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;

static class ArgumentGraphs
{
    static Dictionary<Type, GraphType> entries = new Dictionary<Type, GraphType>();

    static ArgumentGraphs()
    {
        GraphTypeTypeRegistry.Register(typeof(Comparison), typeof(ComparisonGraph));
        GraphTypeTypeRegistry.Register(typeof(StringComparison), typeof(StringComparisonGraph));
        Add<StringComparisonGraph>();
        Add<WhereExpressionGraph>();
        Add<OrderByGraph>();
        Add<ComparisonGraph>();
    }

    public static void RegisterInContainer(Action<Type, GraphType> registerInstance)
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
}