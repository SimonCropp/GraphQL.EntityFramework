/// <summary>
/// Caches MethodInfo for common Enumerable methods to avoid repeated reflection
/// </summary>
static class EnumerableMethodCache
{
    static Type enumerableType = typeof(Enumerable);

    // Base MethodInfo instances (before MakeGenericMethod)
    public static readonly MethodInfo OrderByMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "OrderBy" &&
                    _.GetParameters().Length == 2);

    public static readonly MethodInfo SelectMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "Select" &&
                    _.GetParameters().Length == 2);

    public static readonly MethodInfo ToListMethod = enumerableType
        .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!;

    public static readonly MethodInfo AnyMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(_ => _.Name == "Any" &&
                    _.GetParameters().Length == 2);

    // Cache for generic method instances
    static ConcurrentDictionary<(MethodInfo, Type[]), MethodInfo> genericMethodCache = new();

    public static MethodInfo MakeGenericMethod(MethodInfo baseMethod, params Type[] typeArguments)
    {
        var key = (baseMethod, typeArguments);
        return genericMethodCache.GetOrAdd(key, _ => _.Item1.MakeGenericMethod(_.Item2));
    }
}
