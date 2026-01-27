namespace Benchmarks;

public class ParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<ChildEntity> Children { get; set; } = [];
}

public class ChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid? ParentId { get; set; }
    public ParentEntity? Parent { get; set; }
}

public class BenchmarkDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ParentEntity> Parents => Set<ParentEntity>();
    public DbSet<ChildEntity> Children => Set<ChildEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentEntity>();
        modelBuilder.Entity<ChildEntity>()
            .HasOne(_ => _.Parent)
            .WithMany(_ => _.Children)
            .HasForeignKey(_ => _.ParentId);
    }
}

public class ParentGraphType :
    EfObjectGraphType<BenchmarkDbContext, ParentEntity>
{
    public ParentGraphType(IEfGraphQLService<BenchmarkDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationListField(
            name: "children",
            projection: _ => _.Children,
            resolve: _ => _.Projection);
        AutoMap();
    }
}

public class ChildGraphType :
    EfObjectGraphType<BenchmarkDbContext, ChildEntity>
{
    public ChildGraphType(IEfGraphQLService<BenchmarkDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}

public class BenchmarkQuery :
    QueryGraphType<BenchmarkDbContext>
{
    public BenchmarkQuery(IEfGraphQLService<BenchmarkDbContext> efGraphQlService) :
        base(efGraphQlService) =>
        AddQueryField(
            name: "parents",
            resolve: _ => _.DbContext.Parents);
}

public class BenchmarkSchema(IServiceProvider resolver) :
    Schema(resolver)
{
    public BenchmarkSchema(IServiceProvider resolver, BenchmarkQuery query) : this(resolver)
    {
        RegisterTypeMapping(typeof(ParentEntity), typeof(ParentGraphType));
        RegisterTypeMapping(typeof(ChildEntity), typeof(ChildGraphType));
        Query = query;
    }
}

[MemoryDiagnoser]
public class SimpleQueryBenchmark
{
    IDocumentExecuter executer = null!;
    BenchmarkDbContext database = null!;
    IServiceProvider provider = null!;
    ISchema schema = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Create in-memory database
        var builder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        builder.UseInMemoryDatabase("BenchmarkDb");
        database = new(builder.Options);

        // Seed data - 100 parents with 5 children each
        for (var i = 0; i < 100; i++)
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
    public async Task<ExecutionResult> QueryParentsWithChildren()
    {
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

        return result;
    }

    [Benchmark]
    public async Task<ExecutionResult> QueryParentsOnly()
    {
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

        return result;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        database.Dispose();
        (provider as IDisposable)?.Dispose();
    }
}
