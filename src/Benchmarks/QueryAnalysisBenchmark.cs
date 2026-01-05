namespace Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class QueryAnalysisBenchmark
{
    IDocumentExecuter executer = null!;
    BenchmarkDbContext database = null!;
    IServiceProvider provider = null!;
    ISchema schema = null!;
    List<string> executedQueries = [];

    [GlobalSetup]
    public async Task Setup()
    {
        // Create in-memory database with logging
        var builder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        builder.UseInMemoryDatabase("AnalysisDb")
            .LogTo(query => executedQueries.Add(query), Microsoft.Extensions.Logging.LogLevel.Information);

        database = new(builder.Options);

        // Seed data - 10 parents with 5 children each for analysis
        for (var i = 0; i < 10; i++)
        {
            var parent = new ParentEntity
            {
                Property = $"Parent {i}"
            };

            for (var j = 0; j < 5; j++)
            {
                parent.Children.Add(new()
                {
                    Property = $"Child {i}-{j}",
                    Parent = parent
                });
            }

            database.Parents.Add(parent);
        }

        await database.SaveChangesAsync();

        // Setup GraphQL
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

    [Benchmark]
    public async Task<int> QueryWithIncludes()
    {
        executedQueries.Clear();

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

        // Return count to force evaluation without serialization overhead
        var data = result.Data as IDictionary<string, object>;
        var parents = data?["parents"] as IEnumerable<object>;
        return parents?.Count() ?? 0;
    }

    [Benchmark]
    public async Task<int> QueryWithoutIncludes()
    {
        executedQueries.Clear();

        var query = """
            {
              parents {
                id
                property
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
        Console.WriteLine($"\n=== Query Analysis ===");
        Console.WriteLine($"Total EF queries executed: {executedQueries.Count}");
        foreach (var q in executedQueries.Take(10))
        {
            Console.WriteLine(q);
        }

        database.Dispose();
        (provider as IDisposable)?.Dispose();
    }
}
