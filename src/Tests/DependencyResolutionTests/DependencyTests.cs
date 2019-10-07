using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

public class DependencyTests :
    XunitApprovalBase
{
    static SqlInstance<DependencyDbContext> sqlInstance;

    static DependencyTests()
    {
        GraphTypeTypeRegistry.Register<Entity, EntityGraph>();

        sqlInstance = new SqlInstance<DependencyDbContext>(builder => new DependencyDbContext(builder.Options));
    }

    public DependencyTests(ITestOutputHelper output) :
        base(output)
    {
    }

    static string query = @"
{
  entities
  {
    property
  }
}";

    static class ModelBuilder
    {
        static ModelBuilder()
        {
            var builder = new DbContextOptionsBuilder<DependencyDbContext>();
            builder.UseSqlServer("Fake");
            using var context = new DependencyDbContext(builder.Options);
            Instance = context.Model;
        }

        public static readonly IModel Instance;
    }

    [Fact]
    public async Task ExplicitModel()
    {
        using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();

        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (DependencyDbContext) userContext,
            ModelBuilder.Instance);
        using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            UserContext = dbContext,
            Inputs = null
        };

        await ExecutionResultData(executionOptions);
    }

    [Fact]
    public async Task ScopedDbContext()
    {
        using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddScoped(x => { return database.Context; });

        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (DependencyDbContext) userContext);
        using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            UserContext = dbContext,
            Inputs = null
        };

        await ExecutionResultData(executionOptions);
    }

    [Fact]
    public async Task TransientDbContext()
    {
        using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddTransient(x => database.Context);

        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (DependencyDbContext) userContext);
        using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            UserContext = dbContext,
            Inputs = null
        };

        await ExecutionResultData(executionOptions);
    }

    [Fact]
    public async Task SingletonDbContext()
    {
        using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = BuildServiceCollection();
        services.AddSingleton(database.Context);

        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (DependencyDbContext) userContext);
        using var provider = services.BuildServiceProvider();
        using var schema = new DependencySchema(provider);
        var executionOptions = new ExecutionOptions
        {
            Schema = schema,
            Query = query,
            UserContext = dbContext,
            Inputs = null
        };

        await ExecutionResultData(executionOptions);
    }

    static ServiceCollection BuildServiceCollection()
    {
        var services = new ServiceCollection();
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
        var documentExecuter = new EfDocumentExecuter();
        var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
        var data = (Dictionary<string, object>) executionResult.Data;
        var objects = (List<object>) data.Single().Value;
        Assert.Single(objects);
    }
}