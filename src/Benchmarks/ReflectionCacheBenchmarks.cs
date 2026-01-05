using BenchmarkDotNet.Attributes;
using System.Reflection;

[MemoryDiagnoser]
[SimpleJob(iterationCount: 10)]
public class ReflectionCacheBenchmarks
{
    private Type enumerableType = typeof(Enumerable);

    [Benchmark(Baseline = true)]
    public MethodInfo GetOrderByMethod_NoCache()
    {
        // Simulates current SelectExpressionBuilder pattern
        return enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
    }

    [Benchmark]
    public MethodInfo GetSelectMethod_NoCache()
    {
        return enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Select" && m.GetParameters().Length == 2);
    }

    [Benchmark]
    public MethodInfo GetToListMethod_NoCache()
    {
        return enumerableType
            .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!;
    }

    [Benchmark]
    public MethodInfo GetAnyMethod_NoCache()
    {
        // Simulates ExpressionBuilder list filter pattern
        return enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2);
    }

    [Benchmark]
    public MethodInfo MakeGenericOrderByMethod_NoCache()
    {
        var orderBy = enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
        return orderBy.MakeGenericMethod(typeof(string), typeof(int));
    }
}
