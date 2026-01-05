namespace Benchmarks;

[MemoryDiagnoser]
public class PerformanceAnalysisBenchmark
{
    IDocumentExecuter executer = null!;
    BenchmarkDbContext database = null!;
    IServiceProvider provider = null!;
    ISchema schema = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var builder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        builder.UseInMemoryDatabase("PerfAnalysisDb");
        database = new(builder.Options);

        // Seed 100 parents with 5 children each
        for (var i = 0; i < 100; i++)
        {
            var parent = new ParentEntity { Property = $"Parent {i}" };
            for (var j = 0; j < 5; j++)
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

    [Benchmark(Baseline = true)]
    public async Task<int> DirectEFQuery()
    {
        // Direct EF query with projection - what we'd write manually
        var results = await database.Parents
            .Select(p => new
            {
                p.Id,
                p.Property,
                Children = p.Children.Select(c => new { c.Id, c.Property }).ToList()
            })
            .ToListAsync();

        return results.Count;
    }

    [Benchmark]
    public async Task<int> GraphQLQueryWithProjection()
    {
        // GraphQL query - the library should apply projection
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

    [Benchmark]
    public async Task<string> GraphQLQueryWithSerialization()
    {
        // Full GraphQL execution including serialization
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

        // Serialize the result
        var serializer = new GraphQLSerializer();
        using var stream = new MemoryStream();
        await serializer.WriteAsync(stream, result);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        database.Dispose();
        (provider as IDisposable)?.Dispose();
    }
}
