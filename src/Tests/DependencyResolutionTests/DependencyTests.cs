using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class DependencyTests
{
    static SqlInstance<DependencyDbContext> sqlInstance;

    static DependencyTests()
    {
        GraphTypeTypeRegistry.Register<Entity, EntityGraph>();

        sqlInstance = new(builder => new(builder.Options));
    }

    static string query = @"
{
  entities
  {
    property
  }
}";

    [Fact]
    public async Task ExplicitModel()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();

        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => ((UserContextSingleDb<DependencyDbContext>) userContext).DbContext,
            sqlInstance.Model);
        await using var provider = services.BuildServiceProvider();
        using DependencySchema schema = new(provider);
        ExecutionOptions executionOptions = new()
        {
            Schema = schema,
            Query = query,
            UserContext = new UserContextSingleDb<DependencyDbContext>(dbContext),
            Inputs = null
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
            userContext => ((UserContextSingleDb<DependencyDbContext>) userContext).DbContext);
        await using var provider = services.BuildServiceProvider();
        using DependencySchema schema = new(provider);
        ExecutionOptions executionOptions = new()
        {
            Schema = schema,
            Query = query,
            UserContext = new UserContextSingleDb<DependencyDbContext>(dbContext),
            Inputs = null
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
            userContext => ((UserContextSingleDb<DependencyDbContext>) userContext).DbContext);
        await using var provider = services.BuildServiceProvider();
        using DependencySchema schema = new(provider);
        ExecutionOptions executionOptions = new()
        {
            Schema = schema,
            Query = query,
            UserContext = new UserContextSingleDb<DependencyDbContext>(dbContext),
            Inputs = null
        };

        await ExecutionResultData(executionOptions);
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
            userContext => ((UserContextSingleDb<DependencyDbContext>) userContext).DbContext);
        await using var provider = services.BuildServiceProvider();
        using DependencySchema schema = new(provider);
        ExecutionOptions executionOptions = new()
        {
            Schema = schema,
            Query = query,
            UserContext = new UserContextSingleDb<DependencyDbContext>(dbContext),
            Inputs = null
        };

        await ExecutionResultData(executionOptions);
    }

    static ServiceCollection BuildServiceCollection()
    {
        ServiceCollection services = new();
        services.AddSingleton<DependencyQuery>();
        services.AddSingleton(typeof(EntityGraph));
        return services;
    }

    static Task AddData(DependencyDbContext dbContext)
    {
        dbContext.AddRange(new Entity {Property = "A"});
        return dbContext.SaveChangesAsync();
    }

    static async Task ExecutionResultData(ExecutionOptions executionOptions)
    {
        EfDocumentExecuter documentExecuter = new();
        var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
        var data = (Dictionary<string, object>) executionResult.Data;
        var objects = (List<object>) data.Single().Value;
        Assert.Single(objects);
    }
}