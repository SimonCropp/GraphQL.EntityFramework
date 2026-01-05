[MemoryDiagnoser]
[SimpleJob(iterationCount: 10)]
public class ReflectionCacheBenchmarks
{
    Type enumerableType = typeof(Enumerable);

    [Benchmark(Baseline = true)]
    public MethodInfo GetOrderByMethod_NoCache() =>
        // Simulates current SelectExpressionBuilder pattern
        enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == "OrderBy" && _.GetParameters().Length == 2);

    [Benchmark]
    public MethodInfo GetSelectMethod_NoCache() =>
        enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == "Select" && _.GetParameters().Length == 2);

    [Benchmark]
    public MethodInfo GetToListMethod_NoCache() =>
        enumerableType
            .GetMethod("ToList", BindingFlags.Static | BindingFlags.Public)!;

    [Benchmark]
    public MethodInfo GetAnyMethod_NoCache() =>
        // Simulates ExpressionBuilder list filter pattern
        enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == "Any" && _.GetParameters().Length == 2);

    [Benchmark]
    public MethodInfo MakeGenericOrderByMethod_NoCache()
    {
        var orderBy = enumerableType
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(_ => _.Name == "OrderBy" && _.GetParameters().Length == 2);
        return orderBy.MakeGenericMethod(typeof(string), typeof(int));
    }
}
