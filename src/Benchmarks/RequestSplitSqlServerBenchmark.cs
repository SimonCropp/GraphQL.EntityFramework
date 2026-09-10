namespace Benchmarks;

/// <summary>
/// The full request and the EF execution alone against SQL Server LocalDB, to put the share of
/// the library next to a real database round trip.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
public class RequestSplitSqlServerBenchmark
{
    const string simpleQuery = """
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

    static SqlInstance<BenchmarkDbContext> sqlInstance = new(builder => new(builder.Options));

    SqlDatabase<BenchmarkDbContext> database = null!;
    IDocumentExecuter executer = null!;
    IServiceProvider provider = null!;
    ISchema schema = null!;
    IQueryable<ParentEntity> projected = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        database = await sqlInstance.Build();
        var context = database.Context;
        await Seed.Run(context);

        var services = new ServiceCollection();
        services.AddSingleton(context);
        services.AddSingleton<ParentGraphType>();
        services.AddSingleton<ChildGraphType>();
        services.AddSingleton<CapturingQuery>();
        EfGraphQLConventions.RegisterInContainer<BenchmarkDbContext>(
            services,
            (_, _) => context,
            sqlInstance.Model,
            disableTracking: true);
        provider = services.BuildServiceProvider();
        schema = new CapturingSchema(provider, provider.GetRequiredService<CapturingQuery>());
        executer = new EfDocumentExecuter();

        await Full();
        var captured = CapturingQuery.Captured!;
        var model = sqlInstance.Model;
        var includeAppender = new IncludeAppender(
            NavigationReader.GetNavigationProperties(model),
            model.GetKeyNames(),
            ForeignKeyExtractor.GetForeignKeyProperties(model),
            new Dictionary<Type, IReadOnlyList<Type>>());
        projected = includeAppender.ApplyProjection<BenchmarkDbContext, ParentEntity>(captured, null, context.Parents.AsNoTracking());
    }

    [Benchmark(Baseline = true)]
    public async Task<int> Full()
    {
        var result = await executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = simpleQuery;
            options.RequestServices = provider;
        });

        if (result.Errors is { Count: > 0 })
        {
            throw new(string.Join(Environment.NewLine, result.Errors.Select(_ => _.Message)));
        }

        var data = (IDictionary<string, object?>) ((GraphQL.Execution.ExecutionNode) result.Data!).ToValue()!;
        var parents = (IEnumerable<object>) data["parents"]!;
        return parents.Count();
    }

    [Benchmark]
    public Task<List<ParentEntity>> EfExecute() =>
        projected.ToListAsync();

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await database.DisposeAsync();
        (provider as IDisposable)?.Dispose();
    }
}
