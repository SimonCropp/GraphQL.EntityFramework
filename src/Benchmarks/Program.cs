using BenchmarkDotNet.Running;

BenchmarkRunner.Run<ReflectionCacheBenchmarks>();
BenchmarkRunner.Run<NavigationLookupBenchmarks>();
BenchmarkRunner.Run<TypeConverterBenchmarks>();
