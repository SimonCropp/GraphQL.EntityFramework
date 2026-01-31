public partial class IntegrationTests
{

    [Fact]
    public async Task QueryConnection_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var query =
            """
            {
              parentEntitiesConnection {
                edges {
                  node {
                    property
                  }
                }
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "Value1"
        };

        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        dbContext.AddRange(entity);
        await dbContext.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton(database.Context);
        services.AddGraphQL(null);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        EfGraphQLConventions.RegisterInContainer(services, (_, _) => dbContext, dbContext.Model);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        var executer = new EfDocumentExecuter();

        // Create a pre-cancelled token
        var cts = new CancelSource();
        await cts.CancelAsync();

        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            RequestServices = provider,
            CancellationToken = cts.Token
        };

        // Should throw OperationCanceledException (or derived TaskCanceledException) without wrapping
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await executer.ExecuteAsync(options));
    }

    [Fact]
    public async Task QueryConnection_WithOperationCancelled_ThrowsOperationCanceledException()
    {
        var query =
            """
            {
              parentEntitiesConnection {
                edges {
                  node {
                    property
                  }
                }
              }
            }
            """;

        var entity = new ParentEntity
        {
            Property = "Value1"
        };

        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        dbContext.AddRange(entity);
        await dbContext.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton(database.Context);
        services.AddGraphQL(null);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        EfGraphQLConventions.RegisterInContainer(services, (_, _) => dbContext, dbContext.Model);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);
        var executer = new EfDocumentExecuter();

        var cts = new CancelSource();

        var options = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            RequestServices = provider,
            CancellationToken = cts.Token
        };

        // Cancel immediately to trigger OperationCanceledException
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await executer.ExecuteAsync(options));
    }
}
