using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GraphQL.EntityFramework;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

static class ArgumentGraphs
{
    static Dictionary<Type, GraphType> entries = new();

    [ModuleInitializer]
    public static void Initialize()
    {
        Add<StringComparisonGraph>();
        Add<WhereExpressionGraph>();
        Add<OrderByGraph>();
        Add<ComparisonGraph>();
        Add<ConnectorGraph>();
    }

    internal static void RegisterMappings(this ISchema schema)
    {
        schema.RegisterTypeMapping(typeof(Comparison), typeof(ComparisonGraph));
        schema.RegisterTypeMapping(typeof(StringComparison), typeof(StringComparisonGraph));
        schema.RegisterTypeMapping(typeof(Connector), typeof(ComparisonGraph));
    }

    public static void RegisterInContainer(IServiceCollection services)
    {
        foreach (var entry in entries)
        {
            services.AddSingleton(entry.Key, entry.Value);
        }
    }

    static void Add<T>()
        where T : GraphType, new()
    {
        T value = new();
        entries.Add(typeof(T), value);
    }
}