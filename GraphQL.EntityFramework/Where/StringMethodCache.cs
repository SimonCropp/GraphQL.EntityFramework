using System;
using System.Linq;
using System.Reflection;

static class StringMethodCache
{
    public static MethodInfo Any;

    public static MethodInfo Equal = typeof(string).GetMethod("Equals", new[] {typeof(string), typeof(string), typeof(StringComparison)});
    public static MethodInfo StartsWith = typeof(string).GetMethod("StartsWith", new[] {typeof(string), typeof(StringComparison)});
    public static MethodInfo IndexOf = typeof(string).GetMethod("IndexOf", new[] {typeof(string), typeof(StringComparison)});
    public static MethodInfo EndsWith = typeof(string).GetMethod("EndsWith", new[] {typeof(string), typeof(StringComparison)});

    static StringMethodCache()
    {
        Any = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));
    }
}