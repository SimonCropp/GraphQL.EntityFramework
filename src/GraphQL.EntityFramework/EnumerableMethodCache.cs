using System.Collections.Concurrent;
using System.Reflection;

namespace GraphQL.EntityFramework;

/// <summary>
/// Caches MethodInfo for common Enumerable methods to avoid repeated reflection
/// </summary>
static class EnumerableMethodCache
{
    static readonly Type enumerableType = typeof(Enumerable);

    // Base MethodInfo instances (before MakeGenericMethod)
    public static readonly MethodInfo OrderByMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);

    public static readonly MethodInfo SelectMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == "Select" && m.GetParameters().Length == 2);

    public static readonly MethodInfo ToListMethod = enumerableType
        .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!;

    public static readonly MethodInfo AnyMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

    // Cache for generic method instances
    static readonly ConcurrentDictionary<(MethodInfo, Type[]), MethodInfo> genericMethodCache = new();

    public static MethodInfo MakeGenericMethod(MethodInfo baseMethod, params Type[] typeArguments)
    {
        var key = (baseMethod, typeArguments);
        return genericMethodCache.GetOrAdd(key, k => k.Item1.MakeGenericMethod(k.Item2));
    }
}
