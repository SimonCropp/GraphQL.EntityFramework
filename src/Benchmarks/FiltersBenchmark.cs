namespace Benchmarks;

/// <summary>
/// A request of parents, their children and the parent of each child, with filters registered.
/// <see cref="NoFilters"/> has none. <see cref="FiltersForOtherTypes"/> has a filter only for a type
/// the request never returns, the common case, since filters are registered for the few types that
/// need them. Every result used to go through the filters anyway, copying each into lists of
/// candidates and allocating a delegate per item, so it cost more than NoFilters. A result no filter
/// applies to is now returned as it is, so the two should be level. <see cref="FiltersApplied"/>
/// filters every child, keeping them all, for the cost of filtering itself.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
public class FiltersBenchmark
{
    const string query = """
        {
          parents {
            id
            property
            children {
              id
              property
              parent {
                id
              }
            }
          }
        }
        """;

    // Each parent has five children
    [Params(10, 100)]
    public int Parents { get; set; }

    EfDocumentExecuter executer = new();
    BenchmarkDbContext database = null!;
    ServiceProvider noFilters = null!;
    ServiceProvider filtersForOtherTypes = null!;
    ServiceProvider filtersApplied = null!;
    ISchema noFiltersSchema = null!;
    ISchema filtersForOtherTypesSchema = null!;
    ISchema filtersAppliedSchema = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        var builder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        // GlobalSetup runs once per case, and a named in memory database outlives the context
        builder.UseInMemoryDatabase($"Filters{Guid.NewGuid():N}");
        database = new(builder.Options);
        await Seed.Run(database, Parents);

        (noFilters, noFiltersSchema) = Build(null);

        var otherTypes = new Filters<BenchmarkDbContext>();
        otherTypes.For<UnreturnedEntity>().Add(filter: _ => _.Visible);
        (filtersForOtherTypes, filtersForOtherTypesSchema) = Build(otherTypes);

        var applied = new Filters<BenchmarkDbContext>();
        // Every child has a property, so all of them are kept and the response is the same
        applied.For<ChildEntity>().Add(filter: _ => _.Property != null);
        (filtersApplied, filtersAppliedSchema) = Build(applied);
    }

    (ServiceProvider, ISchema) Build(Filters<BenchmarkDbContext>? filters)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ParentGraphType>();
        services.AddSingleton<ChildGraphType>();
        services.AddSingleton<BenchmarkQuery>();
        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, _) => database,
            database.Model,
            _ => filters,
            disableTracking: true);
        var provider = services.BuildServiceProvider();
        return (provider, new BenchmarkSchema(provider, provider.GetRequiredService<BenchmarkQuery>()));
    }

    [Benchmark(Baseline = true)]
    public Task<int> NoFilters() =>
        Execute(noFiltersSchema, noFilters);

    [Benchmark]
    public Task<int> FiltersForOtherTypes() =>
        Execute(filtersForOtherTypesSchema, filtersForOtherTypes);

    [Benchmark]
    public Task<int> FiltersApplied() =>
        Execute(filtersAppliedSchema, filtersApplied);

    async Task<int> Execute(ISchema schema, IServiceProvider provider)
    {
        var result = await executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = query;
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

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await noFilters.DisposeAsync();
        await filtersForOtherTypes.DisposeAsync();
        await filtersApplied.DisposeAsync();
        await database.DisposeAsync();
    }
}

// A type with a filter registered for it that the benchmark's request never returns
public class UnreturnedEntity
{
    public bool Visible { get; set; }
}
