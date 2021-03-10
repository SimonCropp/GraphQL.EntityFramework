using System.Collections.Generic;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class MultiContextTests
{
    [Fact]
    public async Task Run()
    {
        GraphTypeTypeRegistry.Register<Entity1, Entity1Graph>();
        GraphTypeTypeRegistry.Register<Entity2, Entity2Graph>();

        SqlInstance<DbContext1> sqlInstance1 = new(
            constructInstance: builder => new(builder.Options));

        SqlInstance<DbContext2> sqlInstance2 = new(
            constructInstance: builder => new(builder.Options));

        var query = @"
{
  entity1
  {
    property
  },
  entity2
  {
    property
  }
}";

        Entity1 entity1 = new()
        {
            Property = "the entity1"
        };
        Entity2 entity2 = new()
        {
            Property = "the entity2"
        };

        ServiceCollection services = new();

        services.AddSingleton<MultiContextQuery>();
        services.AddSingleton<Entity1Graph>();
        services.AddSingleton<Entity2Graph>();

        await using (var database1 = await sqlInstance1.Build())
        await using (var database2 = await sqlInstance2.Build())
        {
            await database1.AddDataUntracked(entity1);
            await database2.AddDataUntracked(entity2);

            var dbContext1 = database1.NewDbContext();
            var dbContext2 = database2.NewDbContext();
            services.AddSingleton(dbContext1);
            services.AddSingleton(dbContext2);

            #region RegisterMultipleInContainer

            EfGraphQLConventions.RegisterInContainer(
                services,
                userContext => ((UserContext) userContext).DbContext1);
            EfGraphQLConventions.RegisterInContainer(
                services,
                userContext => ((UserContext) userContext).DbContext2);

            #endregion

            await using var provider = services.BuildServiceProvider();
            using MultiContextSchema schema = new(provider);
            EfDocumentExecuter documentExecuter = new();

            #region MultiExecutionOptions

            ExecutionOptions executionOptions = new()
            {
                Schema = schema,
                Query = query,
                UserContext = new UserContext(dbContext1, dbContext2)
            };

            #endregion

            var result = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
            await Verifier.Verify(result.Data);
        }
    }
}

#region MultiUserContext
public class UserContext: Dictionary<string, object>
{
    public UserContext(DbContext1 context1, DbContext2 context2)
    {
        DbContext1 = context1;
        DbContext2 = context2;
    }

    public readonly DbContext1 DbContext1;
    public readonly DbContext2 DbContext2;
}
#endregion