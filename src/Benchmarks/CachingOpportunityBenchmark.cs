namespace Benchmarks;

/// <summary>
/// Demonstrates the potential performance gain from caching projection info and include paths.
/// Currently, these are rebuilt on every query execution even for identical query structures.
/// </summary>
[MemoryDiagnoser]
public class CachingOpportunityBenchmark
{
    IDocumentExecuter executer = null!;
    BenchmarkDbContext database = null!;
    IServiceProvider provider = null!;
    ISchema schema = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var builder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        builder.UseInMemoryDatabase("CachingDb");
        database = new(builder.Options);

        // Small dataset - focusing on query overhead, not data volume
        for (var i = 0; i < 10; i++)
        {
            var parent = new ParentEntity { Property = $"Parent {i}" };
            for (var j = 0; j < 3; j++)
            {
                parent.Children.Add(new() { Property = $"Child {i}-{j}", Parent = parent });
            }
            database.Parents.Add(parent);
        }

        await database.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddSingleton(database);
        services.AddSingleton<BenchmarkDbContext>(_ => database);
        services.AddSingleton<ParentGraphType>();
        services.AddSingleton<ChildGraphType>();
        services.AddSingleton<BenchmarkQuery>();

        EfGraphQLConventions.RegisterInContainer<BenchmarkDbContext>(
            services,
            (_, _) => database,
            database.Model);

        provider = services.BuildServiceProvider();
        var query = provider.GetRequiredService<BenchmarkQuery>();
        schema = new BenchmarkSchema(provider, query);
        executer = new EfDocumentExecuter();
    }

    [Benchmark(Description = "Repeated identical queries (would benefit from caching)")]
    public async Task<int> RepeatedIdenticalQueries()
    {
        // Same query structure executed 100 times
        // Without caching: GetProjectionInfo and GetPaths are called 100 times
        // With caching: Would only be called once (or very few times)
        var query = """
            {
              parents {
                id
                property
                children {
                  id
                  property
                }
              }
            }
            """;

        var totalCount = 0;
        for (var i = 0; i < 100; i++)
        {
            var result = await executer.ExecuteAsync(options =>
            {
                options.Schema = schema;
                options.Query = query;
                options.RequestServices = provider;
            });

            var data = result.Data as IDictionary<string, object>;
            var parents = data?["parents"] as IEnumerable<object>;
            totalCount += parents?.Count() ?? 0;
        }

        return totalCount;
    }

    [Benchmark(Description = "Single complex query (caching wouldn't help)")]
    public async Task<int> SingleComplexQuery()
    {
        // Caching wouldn't help here - only executed once
        var query = """
            {
              parents {
                id
                property
                children {
                  id
                  property
                }
              }
            }
            """;

        var result = await executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = query;
            options.RequestServices = provider;
        });

        var data = result.Data as IDictionary<string, object>;
        var parents = data?["parents"] as IEnumerable<object>;
        return parents?.Count() ?? 0;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        database.Dispose();
        (provider as IDisposable)?.Dispose();
    }
}
