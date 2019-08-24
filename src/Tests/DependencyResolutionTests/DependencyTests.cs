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

    [Fact]
    public async Task Simple()
    {
        var query = @"
{
  entities
  {
    property
  }
}";

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
        dbContext.AddRange(new Entity {Property = "A"});
        await dbContext.SaveChangesAsync();
        var services = new ServiceCollection();
        services.AddSingleton<DependencyQuery>();
        services.AddSingleton(database.Context);
        services.AddSingleton(typeof(EntityGraph));

        return await ExecuteQuery(query, services, dbContext, inputs, filters);
    }
    public static async Task<object> ExecuteQuery<TDbContext>(
        string query,
        ServiceCollection services,
        TDbContext dbContext,
        Inputs inputs,
        GlobalFilters filters)
        where TDbContext : DbContext
    {
        query = query.Replace("'", "\"");
        EfGraphQLConventions.RegisterInContainer(
            services,
            userContext => (TDbContext) userContext,
            provider => filters);
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        using (var provider = services.BuildServiceProvider())
        using (var schema = new DependencySchema(new FuncDependencyResolver(provider.GetRequiredService)))
        {
            var documentExecuter = new EfDocumentExecuter();

            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = query,
                UserContext = dbContext,
                Inputs = inputs
            };

            var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
            return executionResult.Data;
        }
    }
}