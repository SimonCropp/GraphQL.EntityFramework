using BenchmarkDotNet.Attributes;
using System.Collections;

[MemoryDiagnoser]
[SimpleJob(iterationCount: 10)]
public class TypeConverterBenchmarks
{
    private string[] guidStrings = null!;
    private string[] intStrings = null!;
    private string[] stringValues = null!;
    private Dictionary<Type, Func<string[], IList>> converterCache = null!;

    [GlobalSetup]
    public void Setup()
    {
        guidStrings = Enumerable.Range(0, 10)
            .Select(_ => Guid.NewGuid().ToString())
            .ToArray();

        intStrings = Enumerable.Range(0, 10)
            .Select(i => i.ToString())
            .ToArray();

        stringValues = Enumerable.Range(0, 10)
            .Select(i => $"Value{i}")
            .ToArray();

        // Pre-populate converter cache
        converterCache = new()
        {
            [typeof(Guid)] = values => values.Select(Guid.Parse).Cast<object>().ToList(),
            [typeof(int)] = values => values.Select(int.Parse).Cast<object>().ToList(),
            [typeof(string)] = values => values.Cast<object>().ToList(),
            [typeof(DateTime)] = values => values.Select(DateTime.Parse).Cast<object>().ToList(),
            [typeof(bool)] = values => values.Select(bool.Parse).Cast<object>().ToList(),
        };
    }

    [Benchmark(Baseline = true)]
    public IList ConvertGuids_IfElseChain()
    {
        var type = typeof(Guid);
        // Simulates current TypeConverter pattern
        if (type == typeof(Guid))
            return guidStrings.Select(Guid.Parse).Cast<object>().ToList();
        else if (type == typeof(string))
            return guidStrings.Cast<object>().ToList();
        else if (type == typeof(int))
            return guidStrings.Select(int.Parse).Cast<object>().ToList();
        // ... 30+ more type checks
        throw new NotSupportedException();
    }

    [Benchmark]
    public IList ConvertInts_IfElseChain()
    {
        var type = typeof(int);
        if (type == typeof(Guid))
            return intStrings.Select(Guid.Parse).Cast<object>().ToList();
        else if (type == typeof(string))
            return intStrings.Cast<object>().ToList();
        else if (type == typeof(int))
            return intStrings.Select(int.Parse).Cast<object>().ToList();
        throw new NotSupportedException();
    }

    [Benchmark]
    public IList ConvertStrings_IfElseChain()
    {
        var type = typeof(string);
        if (type == typeof(Guid))
            return stringValues.Select(Guid.Parse).Cast<object>().ToList();
        else if (type == typeof(string))
            return stringValues.Cast<object>().ToList();
        else if (type == typeof(int))
            return stringValues.Select(int.Parse).Cast<object>().ToList();
        throw new NotSupportedException();
    }

    [Benchmark]
    public IList ConvertGuids_DictionaryLookup()
    {
        if (converterCache.TryGetValue(typeof(Guid), out var converter))
            return converter(guidStrings);
        throw new NotSupportedException();
    }

    [Benchmark]
    public IList ConvertInts_DictionaryLookup()
    {
        if (converterCache.TryGetValue(typeof(int), out var converter))
            return converter(intStrings);
        throw new NotSupportedException();
    }

    [Benchmark]
    public IList ConvertStrings_DictionaryLookup()
    {
        if (converterCache.TryGetValue(typeof(string), out var converter))
            return converter(stringValues);
        throw new NotSupportedException();
    }
}
