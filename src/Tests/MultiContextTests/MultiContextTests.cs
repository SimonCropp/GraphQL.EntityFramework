public class MultiContextTests
{
    [Fact]
    public async Task Run()
    {
        var sqlInstance1 = new SqlInstance<DbContext1>(constructInstance: builder => new(builder.Options));

        var sqlInstance2 = new SqlInstance<DbContext2>(constructInstance: builder => new(builder.Options));

        var query =
            """
            {
              entity1
              {
                property
              },
              entity2
              {
                property
              }
            }
            """;

        var entity1 = new Entity1
        {
            Property = "the entity1"
        };
        var entity2 = new Entity2
        {
            Property = "the entity2"
        };

        var services = new ServiceCollection();

        services.AddSingleton<MultiContextQuery>();
        services.AddSingleton<Entity1GraphType>();
        services.AddSingleton<Entity2GraphType>();

        await using var database1 = await sqlInstance1.Build();
        await using var database2 = await sqlInstance2.Build();
        await database1.AddDataUntracked(entity1);
        await database2.AddDataUntracked(entity2);

        var dbContext1 = database1.NewDbContext();
        var dbContext2 = database2.NewDbContext();
        services.AddSingleton(dbContext1);
        services.AddSingleton(dbContext2);

        #region RegisterMultipleInContainer

        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, requestServices) => requestServices!.GetRequiredService<DbContext1>());
        EfGraphQLConventions.RegisterInContainer(
            services,
            (_, requestServices) => requestServices!.GetRequiredService<DbContext2>());

        #endregion

        await using var provider = services.BuildServiceProvider();
        using var schema = new MultiContextSchema(provider);
        var documentExecuter = new EfDocumentExecuter();

        #region MultiExecutionOptions

        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            RequestServices = provider,
        };

        #endregion

        var result = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
        await Verify(result.Serialize());
    }
}