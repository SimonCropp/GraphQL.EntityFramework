using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

public class DependencyTests :
    XunitLoggingBase
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

    [Fact]
    public async Task Simple()
    {
        using (var database = await sqlInstance.Build())
        {
            var result = await RunQuery(database, query, null, null);
            ObjectApprover.Verify(result);
        }
    }

    static async Task<object> RunQuery(
        SqlDatabase<DependencyDbContext> database,
        string query,
        Inputs inputs,
        GlobalFilters filters)
    {
        var dbContext = database.Context;
        await AddData(dbContext);
        var services = new ServiceCollection();
        services.AddSingleton<DependencyQuery>();
        services.AddSingleton(typeof(EntityGraph));
        services.AddSingleton(database.Context);

        return await ExecuteQuery(query, services, dbContext, inputs, filters);
    }

    static async Task<object> ExecuteQuery(
        string query,
        ServiceCollection services,
        DependencyDbContext dbContext,
        Inputs inputs,
        GlobalFilters filters)
    {
        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (DependencyDbContext) userContext,
            provider => filters);
        using (var provider = services.BuildServiceProvider())
        using (var schema = new DependencySchema(provider))
        {
            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = query,
                UserContext = dbContext,
                Inputs = inputs
            };

            return await ExecutionResultData(executionOptions);
        }
    }

    static Task AddData(DependencyDbContext dbContext)
    {
        dbContext.AddRange(new Entity {Property = "A"});
        return dbContext.SaveChangesAsync();
    }

    static async Task<object> ExecutionResultData(ExecutionOptions executionOptions)
    {
        var documentExecuter = new EfDocumentExecuter();
        var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
        return executionResult.Data;
    }
}