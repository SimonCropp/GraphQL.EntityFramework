using System.Collections.Concurrent;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using ExecutionContext = GraphQL.Execution.ExecutionContext;
using IModel = Microsoft.EntityFrameworkCore.Metadata.IModel;

namespace Benchmarks;

/// <summary>
/// A request whose navigation fields resolve once per row. <see cref="FromContainer"/> resolves the
/// DbContext from the request's container, as the default resolver does. <see cref="FromUserContext"/>
/// is handed the one the request already resolved, so its resolves cost next to nothing. The
/// DbContext used to be resolved again by every field of every row, which cost FromContainer a
/// lookup per row, and with a transient DbContext a DbContext per row. It is now resolved once per
/// execution, so the two should be level at every row count. The DbContext resolves column is the
/// number one request makes. More parents than this measures the in memory provider, which scans
/// every child for each parent.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
[Config(typeof(DbContextResolvesConfig))]
public class DbContextResolutionBenchmark
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

    const string dbContextKey = "dbContext";

    // Each parent has five children, so a request resolves the children of every parent and the
    // parent of every child
    [Params(10, 100)]
    public int Parents { get; set; }

    [Params(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
    public ServiceLifetime Lifetime { get; set; }

    EfDocumentExecuter executer = new();
    ServiceProvider fromContainer = null!;
    ServiceProvider fromUserContext = null!;
    ISchema fromContainerSchema = null!;
    ISchema fromUserContextSchema = null!;
    int resolves;

    [GlobalSetup]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            // GlobalSetup runs once per case, and a named in memory database outlives the context
            .UseInMemoryDatabase($"DbContextResolution{Guid.NewGuid():N}")
            .Options;
        IModel model;
        await using (var database = new BenchmarkDbContext(options))
        {
            model = database.Model;
            await Seed.Run(database, Parents);
        }

        (fromContainer, fromContainerSchema) = Build(
            options,
            model,
            Lifetime,
            (_, requestServices) =>
            {
                resolves++;
                return requestServices!.GetRequiredService<BenchmarkDbContext>();
            });
        (fromUserContext, fromUserContextSchema) = Build(
            options,
            model,
            Lifetime,
            (userContext, _) =>
            {
                resolves++;
                return (BenchmarkDbContext) ((IDictionary<string, object?>) userContext)[dbContextKey]!;
            });

        // One request each, for the resolves column
        resolves = 0;
        await FromContainer();
        DbContextResolvesColumn.Record(nameof(FromContainer), Parents, Lifetime, resolves);
        resolves = 0;
        await FromUserContext();
        DbContextResolvesColumn.Record(nameof(FromUserContext), Parents, Lifetime, resolves);
    }

    static (ServiceProvider, ISchema) Build(
        DbContextOptions<BenchmarkDbContext> options,
        IModel model,
        ServiceLifetime lifetime,
        ResolveDbContext<BenchmarkDbContext> resolveDbContext)
    {
        IServiceCollection services = new ServiceCollection();
        // Disposed with the request's scope, whether one per request or one per resolve
        services.Add(new ServiceDescriptor(typeof(BenchmarkDbContext), _ => new BenchmarkDbContext(options), lifetime));
        services.AddSingleton<ParentGraphType>();
        services.AddSingleton<ChildGraphType>();
        services.AddSingleton<BenchmarkQuery>();
        EfGraphQLConventions.RegisterInContainer(
            services,
            resolveDbContext,
            model,
            disableTracking: true);
        var provider = services.BuildServiceProvider();
        return (provider, new BenchmarkSchema(provider, provider.GetRequiredService<BenchmarkQuery>()));
    }

    [Benchmark(Baseline = true)]
    public async Task<int> FromUserContext()
    {
        await using var scope = fromUserContext.CreateAsyncScope();
        var requestServices = scope.ServiceProvider;
        // The request's one lookup, made by the request rather than the library
        var dbContext = requestServices.GetRequiredService<BenchmarkDbContext>();
        return await Execute(
            fromUserContextSchema,
            requestServices,
            new()
            {
                [dbContextKey] = dbContext
            });
    }

    [Benchmark]
    public async Task<int> FromContainer()
    {
        await using var scope = fromContainer.CreateAsyncScope();
        return await Execute(fromContainerSchema, scope.ServiceProvider, []);
    }

    async Task<int> Execute(ISchema schema, IServiceProvider requestServices, Dictionary<string, object?> userContext)
    {
        var result = await executer.ExecuteAsync(options =>
        {
            options.Schema = schema;
            options.Query = query;
            options.RequestServices = requestServices;
            options.UserContext = userContext;
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
        await fromContainer.DisposeAsync();
        await fromUserContext.DisposeAsync();
    }
}

/// <summary>
/// The DbContext resolves of one request on their own, for as many fields as a request of 10, 100
/// and 500 parents of five children has. <see cref="PerField"/> is what every field did, a lookup
/// in the request's container. <see cref="PerExecution"/> is <see cref="IEfGraphQLService{TDbContext}.ResolveDbContext"/>,
/// which looks it up for the first field and hands the rest the one it holds for the execution.
/// A scoped DbContext makes each lookup a dictionary read. A transient one, or a resolver that
/// creates a DbContext, made every field create a DbContext of its own. Each request creates its
/// scope and execution context the same way in both.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
public class DbContextResolveBenchmark
{
    [Params(61, 601, 3001)]
    public int Fields { get; set; }

    [Params(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
    public ServiceLifetime Lifetime { get; set; }

    ServiceProvider provider = null!;
    IEfGraphQLService<BenchmarkDbContext> service = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BenchmarkDbContext>()
            .UseInMemoryDatabase($"DbContextResolve{Guid.NewGuid():N}")
            .Options;
        IModel model;
        using (var database = new BenchmarkDbContext(options))
        {
            model = database.Model;
        }

        IServiceCollection services = new ServiceCollection();
        services.Add(new ServiceDescriptor(typeof(BenchmarkDbContext), _ => new BenchmarkDbContext(options), Lifetime));
        // No resolver, so the DbContext comes from the request's container, as it does by default
        EfGraphQLConventions.RegisterInContainer<BenchmarkDbContext>(services, model: model);
        provider = services.BuildServiceProvider();
        service = provider.GetRequiredService<IEfGraphQLService<BenchmarkDbContext>>();
    }

    [Benchmark(Baseline = true)]
    public BenchmarkDbContext PerField()
    {
        using var scope = provider.CreateScope();
        var fieldContext = FieldContext(scope);
        BenchmarkDbContext dbContext = null!;
        for (var field = 0; field < Fields; field++)
        {
            dbContext = fieldContext.ExecutionContext.RequestServices!.GetRequiredService<BenchmarkDbContext>();
        }

        return dbContext;
    }

    [Benchmark]
    public BenchmarkDbContext PerExecution()
    {
        using var scope = provider.CreateScope();
        var fieldContext = FieldContext(scope);
        BenchmarkDbContext dbContext = null!;
        for (var field = 0; field < Fields; field++)
        {
            dbContext = service.ResolveDbContext(fieldContext);
        }

        return dbContext;
    }

    static ResolveFieldContext FieldContext(IServiceScope scope) =>
        new()
        {
            ExecutionContext = new ExecutionContext
            {
                RequestServices = scope.ServiceProvider
            }
        };

    [GlobalCleanup]
    public void Cleanup() =>
        provider.Dispose();
}

public class DbContextResolvesConfig : ManualConfig
{
    public DbContextResolvesConfig() =>
        AddColumn(new DbContextResolvesColumn());
}

/// <summary>
/// The DbContext resolves one request makes, recorded by the setup of each case. The benchmarks run
/// in process, so the column reads what the setup recorded.
/// </summary>
public class DbContextResolvesColumn : IColumn
{
    static ConcurrentDictionary<(string Method, int Parents, ServiceLifetime Lifetime), int> counts = new();

    public static void Record(string method, int parents, ServiceLifetime lifetime, int count) =>
        counts[(method, parents, lifetime)] = count;

    public string Id => nameof(DbContextResolvesColumn);
    public string ColumnName => "DbContext resolves";
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Custom;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => "DbContext resolves made by one request";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var method = benchmarkCase.Descriptor.WorkloadMethod.Name;
        var parents = (int) benchmarkCase.Parameters["Parents"];
        var lifetime = (ServiceLifetime) benchmarkCase.Parameters["Lifetime"];
        if (counts.TryGetValue((method, parents, lifetime), out var count))
        {
            return count.ToString();
        }

        return "?";
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) =>
        GetValue(summary, benchmarkCase);

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public bool IsAvailable(Summary summary) => true;
}
