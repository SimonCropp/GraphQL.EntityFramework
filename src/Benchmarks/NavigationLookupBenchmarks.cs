[MemoryDiagnoser]
[SimpleJob(iterationCount: 10)]
public class NavigationLookupBenchmarks
{
    private List<Navigation> navigationList = null!;
    private Dictionary<string, Navigation> navigationDict = null!;
    private const int NavigationCount = 20;

    [GlobalSetup]
    public void Setup()
    {
        // Simulate typical entity with 20 navigation properties
        navigationList = Enumerable.Range(0, NavigationCount)
            .Select(_ => new Navigation(
                $"Property{_}",
                typeof(string),
                IsCollection: true,
                IsNullable: false))
            .ToList();

        navigationDict = navigationList
            .ToDictionary(_ => _.Name.ToLowerInvariant(), n => n);
    }

    [Benchmark(Baseline = true)]
    public Navigation? LinearSearch_FirstItem() =>
        // Current pattern - search at beginning
        navigationList.FirstOrDefault(_ =>
            _.Name.Equals("Property0", StringComparison.OrdinalIgnoreCase));

    [Benchmark]
    public Navigation? LinearSearch_MiddleItem() =>
        // Current pattern - search in middle
        navigationList.FirstOrDefault(_ =>
            _.Name.Equals("Property10", StringComparison.OrdinalIgnoreCase));

    [Benchmark]
    public Navigation? LinearSearch_LastItem() =>
        // Current pattern - worst case
        navigationList.FirstOrDefault(n =>
            n.Name.Equals("Property19", StringComparison.OrdinalIgnoreCase));

    [Benchmark]
    public Navigation? DictionaryLookup_FirstItem()
    {
        navigationDict.TryGetValue("property0", out var result);
        return result;
    }

    [Benchmark]
    public Navigation? DictionaryLookup_MiddleItem()
    {
        navigationDict.TryGetValue("property10", out var result);
        return result;
    }

    [Benchmark]
    public Navigation? DictionaryLookup_LastItem()
    {
        navigationDict.TryGetValue("property19", out var result);
        return result;
    }
}
