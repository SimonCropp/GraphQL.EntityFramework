namespace Benchmarks;

/// <summary>
/// Splits a request into its stages so the share of the library can be read off: GraphQL.NET
/// execution alone for the same result shape, the full request through the library, and the
/// stages the library adds (argument trees, the projection tree, the EF execution of that tree).
/// The residual, Full less GraphQLNetOnly less ApplyProjection less EfExecute, is the per row
/// work in the resolvers.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[WarmupCount(3)]
[IterationCount(10)]
public class RequestSplitBenchmark
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

    const string argumentsQuery = """
        {
          parents(where: {property: {startsWith: "Parent 1"}}, orderBy: property) {
            id
            property
            children(orderBy: property_desc) {
              id
              property
            }
          }
        }
        """;

    const string inMemoryArgumentsQuery = """
        {
          parents {
            id
            property
            childrenReversed(where: {property: {startsWith: "Child"}}, orderBy: property_desc) {
              id
              property
            }
          }
        }
        """;

    const string fragmentsQuery = """
        {
          parents {
            ...parentFields
            children {
              ... on Child {
                id
                property
              }
            }
          }
        }
        fragment parentFields on Parent {
          id
          property
        }
        """;

    IDocumentExecuter executer = null!;
    BenchmarkDbContext database = null!;
    IServiceProvider provider = null!;
    ISchema librarySchema = null!;
    ISchema plainSchema = null!;
    IncludeAppender includeAppender = null!;
    IResolveFieldContext capturedSimple = null!;
    IResolveFieldContext capturedArguments = null!;
    IQueryable<ParentEntity> projected = null!;
    List<string> keyNames = ["Id"];

    [GlobalSetup]
    public async Task Setup()
    {
        var builder = new DbContextOptionsBuilder<BenchmarkDbContext>();
        // GlobalSetup runs once per benchmark method, and a named in memory database outlives the
        // context, so a fixed name would seed on top of the previous method's data
        builder.UseInMemoryDatabase($"RequestSplit{Guid.NewGuid():N}");
        database = new(builder.Options);
        await Seed.Run(database);

        var services = new ServiceCollection();
        services.AddSingleton(database);
        services.AddSingleton<ParentGraphType>();
        services.AddSingleton<ChildGraphType>();
        services.AddSingleton<CapturingQuery>();
        EfGraphQLConventions.RegisterInContainer<BenchmarkDbContext>(
            services,
            (_, _) => database,
            database.Model,
            disableTracking: true);
        provider = services.BuildServiceProvider();
        librarySchema = new CapturingSchema(provider, provider.GetRequiredService<CapturingQuery>());
        executer = new EfDocumentExecuter();

        var plainParents = await database.Parents
            .Include(_ => _.Children)
            .AsNoTracking()
            .ToListAsync();
        plainSchema = new PlainSchema(plainParents);

        var model = database.Model;
        includeAppender = new(
            NavigationReader.GetNavigationProperties(model),
            model.GetKeyNames(),
            ForeignKeyExtractor.GetForeignKeyProperties(model),
            new Dictionary<Type, IReadOnlyList<Type>>());

        // One execution per shape, to capture the field context the stages replay
        await Execute(librarySchema, simpleQuery);
        capturedSimple = CapturingQuery.Captured!;
        await Execute(librarySchema, argumentsQuery);
        capturedArguments = CapturingQuery.Captured!;
        await Execute(librarySchema, fragmentsQuery);
        await Execute(librarySchema, inMemoryArgumentsQuery);
        await Execute(plainSchema, simpleQuery);

        projected = includeAppender.ApplyProjection<BenchmarkDbContext, ParentEntity>(capturedSimple, null, Parents);
    }

    IQueryable<ParentEntity> Parents => database.Parents.AsNoTracking();

    async Task<int> Execute(ISchema schema, string query)
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

    [Benchmark]
    public Task<int> GraphQLNetOnly() =>
        Execute(plainSchema, simpleQuery);

    [Benchmark(Baseline = true)]
    public Task<int> Full() =>
        Execute(librarySchema, simpleQuery);

    [Benchmark]
    public Task<int> FullWithArguments() =>
        Execute(librarySchema, argumentsQuery);

    [Benchmark]
    public Task<int> FullWithFragments() =>
        Execute(librarySchema, fragmentsQuery);

    /// <summary>
    /// The where and orderBy of a navigation whose resolver returns a collection other than the
    /// projected one, so they are evaluated in memory in the resolver, once per parent row.
    /// </summary>
    [Benchmark]
    public Task<int> FullWithInMemoryArguments() =>
        Execute(librarySchema, inMemoryArgumentsQuery);

    [Benchmark]
    public System.Linq.Expressions.Expression ApplyArguments() =>
        Parents.ApplyGraphQlArguments(capturedArguments, keyNames, true, false).Expression;

    [Benchmark]
    public System.Linq.Expressions.Expression ApplyProjection() =>
        includeAppender.ApplyProjection<BenchmarkDbContext, ParentEntity>(capturedSimple, null, Parents).Expression;

    [Benchmark]
    public System.Linq.Expressions.Expression ApplyProjectionWithArguments()
    {
        var query = Parents.ApplyGraphQlArguments(capturedArguments, keyNames, true, false);
        return includeAppender.ApplyProjection<BenchmarkDbContext, ParentEntity>(capturedArguments, null, query).Expression;
    }

    [Benchmark]
    public Task<List<ParentEntity>> EfExecute() =>
        projected.ToListAsync();

    [GlobalCleanup]
    public void Cleanup()
    {
        database.Dispose();
        (provider as IDisposable)?.Dispose();
    }
}

public class CapturingQuery :
    QueryGraphType<BenchmarkDbContext>
{
    public static ResolveEfFieldContext<BenchmarkDbContext, object>? Captured;

    public CapturingQuery(IEfGraphQLService<BenchmarkDbContext> efGraphQlService) :
        base(efGraphQlService) =>
        AddQueryField(
            name: "parents",
            resolve: _ =>
            {
                Captured = _;
                return _.DbContext.Parents;
            });
}

public class CapturingSchema :
    Schema
{
    public CapturingSchema(IServiceProvider resolver, CapturingQuery query) :
        base(resolver)
    {
        RegisterTypeMapping(typeof(ParentEntity), typeof(ParentGraphType));
        RegisterTypeMapping(typeof(ChildEntity), typeof(ChildGraphType));
        Query = query;
    }
}

public class PlainParentGraph :
    ObjectGraphType<ParentEntity>
{
    public PlainParentGraph()
    {
        Name = "Parent";
        Field(_ => _.Id);
        Field(_ => _.Property, nullable: true);
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<PlainChildGraph>>>>("children")
            .Resolve(_ => _.Source.Children);
    }
}

public class PlainChildGraph :
    ObjectGraphType<ChildEntity>
{
    public PlainChildGraph()
    {
        Name = "Child";
        Field(_ => _.Id);
        Field(_ => _.Property, nullable: true);
    }
}

public class PlainQuery :
    ObjectGraphType
{
    public PlainQuery(List<ParentEntity> parents) =>
        Field<NonNullGraphType<ListGraphType<NonNullGraphType<PlainParentGraph>>>>("parents")
            .Resolve(_ => parents);
}

public class PlainSchema :
    Schema
{
    public PlainSchema(List<ParentEntity> parents)
    {
        RegisterTypeMapping(typeof(ParentEntity), typeof(PlainParentGraph));
        RegisterTypeMapping(typeof(ChildEntity), typeof(PlainChildGraph));
        Query = new PlainQuery(parents);
    }
}

public static class Seed
{
    public static async Task Run(BenchmarkDbContext database)
    {
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
    }
}
