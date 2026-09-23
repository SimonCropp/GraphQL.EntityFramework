public partial class IntegrationTests
{
    // The root field and the navigations of every row share one resolve of the DbContext, and a
    // second execution resolves its own
    [Fact]
    public async Task DbContext_resolved_once_per_execution()
    {
        var query =
            """
            {
              parentEntities
              {
                property
                children
                {
                  property
                  parent
                  {
                    property
                  }
                }
              }
            }
            """;

        var (entities, _, _) = BuildParentsWithChildren();

        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton<Mutation>();
        services.AddSingleton(database.Context);
        services.AddGraphQL(null);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        await using var context = database.NewDbContext();
        var resolves = 0;
        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, _) =>
            {
                resolves++;
                return context;
            },
            context.Model);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        var executer = new EfDocumentExecuter();

        await Execute();
        Assert.Equal(1, resolves);

        await Execute();
        Assert.Equal(2, resolves);

        Task<ExecutionResult> Execute() =>
            executer.ExecuteWithErrorCheck(
                new()
                {
                    Schema = schema,
                    Query = query,
                    RequestServices = provider
                });
    }

    // A field context built outside an execution, such as the ResolveEfFieldContext handed to a
    // query field's resolve, has no execution to hold the DbContext against. It used to throw; it
    // is resolved from the context's own request services instead, and not held.
    [Fact]
    public async Task DbContext_resolved_without_an_execution()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        await using var provider = services.BuildServiceProvider();

        var resolves = 0;
        var service = new EfGraphQLService<IntegrationDbContext>(
            dbContext.Model,
            (_, requestServices) =>
            {
                resolves++;
                return requestServices!.GetRequiredService<IntegrationDbContext>();
            });
        var fieldContext = new ResolveFieldContext
        {
            RequestServices = provider
        };

        Assert.Same(dbContext, service.ResolveDbContext(fieldContext));
        Assert.Same(dbContext, service.ResolveDbContext(fieldContext));
        Assert.Equal(2, resolves);
    }
}
