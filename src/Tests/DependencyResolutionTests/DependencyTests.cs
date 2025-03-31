public class DependencyTests
{
    static SqlInstance<DependencyDbContext> sqlInstance;

    static DependencyTests() =>
        sqlInstance = new(builder => new(builder.Options));

    static string query = """
        {
          entities
          {
            property
          }
        }
        """;

    [Fact]
    public async Task ExplicitModel()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddSingleton<DependencySchema>();
        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, _) => dbContext,
            sqlInstance.Model);
        await using var provider = services.BuildServiceProvider();
        using var schema = provider.GetRequiredService<DependencySchema>();
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            Variables = null,
            RequestServices = provider
        };

        await ExecutionResultData(executionOptions);
    }

    [Fact]
    public async Task ScopedDbContext()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddScoped(_ => database.Context);

        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, requestServices) => requestServices!.GetRequiredService<DependencyDbContext>());
        await using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            Variables = null,
            RequestServices = provider
        };

        await ExecutionResultData(executionOptions);
    }

    [Fact]
    public async Task TransientDbContext()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddTransient(_ => database.Context);

        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, requestServices) => requestServices!.GetRequiredService<DependencyDbContext>());
        await using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            Variables = null,
            RequestServices = provider
        };

        await ExecutionResultData(options);
    }

    [Fact]
    public async Task SingletonDbContext()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddSingleton(database.Context);

        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, requestServices) => requestServices!.GetRequiredService<DependencyDbContext>());
        await using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            Variables = null,
            RequestServices = provider
        };

        await ExecutionResultData(options);
    }

    static ServiceCollection BuildServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DependencyQuery>();
        services.AddSingleton(typeof(EntityGraphType));
        return services;
    }

    static Task AddData(DependencyDbContext dbContext)
    {
        dbContext.AddRange(new Entity {Property = "A"});
        return dbContext.SaveChangesAsync();
    }

    static async Task ExecutionResultData(
        ExecutionOptions executionOptions,
        [CallerFilePath] string sourceFile = "")
    {
        var executer = new EfDocumentExecuter();
        var result = await executer.ExecuteWithErrorCheck(executionOptions);
        await Verify(result.Serialize(), sourceFile: sourceFile).ScrubInlineGuids();
    }
}