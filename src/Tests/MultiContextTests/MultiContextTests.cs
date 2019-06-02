using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class MultiContextTests:
    XunitLoggingBase
{
    [Fact]
    public async Task Run()
    {
        GraphTypeTypeRegistry.Register<Entity1, Entity1Graph>();
        GraphTypeTypeRegistry.Register<Entity2, Entity2Graph>();

        var sqlInstance1 = new SqlInstance<DbContext1>(
            constructInstance: builder => new DbContext1(builder.Options));

        var sqlInstance2 = new SqlInstance<DbContext2>(
            constructInstance: builder => new DbContext2(builder.Options));

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

        var entity1 = new Entity1
        {
            Property = "the entity1"
        };
        var entity2 = new Entity2
        {
            Property = "the entity2"
        };

        var sqlDatabase1 = await sqlInstance1.Build();
        var sqlDatabase2 = await sqlInstance2.Build();
        using (var dbContext = sqlDatabase1.NewDbContext())
        {
            dbContext.AddRange(entity1);
            await dbContext.SaveChangesAsync();
        }

        using (var dbContext = sqlDatabase2.NewDbContext())
        {
            dbContext.AddRange(entity2);
            await dbContext.SaveChangesAsync();
        }

        using (var dbContext1 = sqlDatabase1.NewDbContext())
        using (var dbContext2 = sqlDatabase2.NewDbContext())
        {
            var services = new ServiceCollection();

            services.AddSingleton<MultiContextQuery>();
            services.AddSingleton(typeof(Entity1Graph));
            services.AddSingleton(typeof(Entity2Graph));

            #region RegisterMultipleInContainer
            EfGraphQLConventions.RegisterInContainer(
                services,
                dbContext1,
                userContext => ((UserContext) userContext).DbContext1);
            EfGraphQLConventions.RegisterInContainer(
                services,
                dbContext2,
                userContext => ((UserContext) userContext).DbContext2);
            #endregion

            using (var provider = services.BuildServiceProvider())
            using (var schema = new MultiContextSchema(new FuncDependencyResolver(provider.GetRequiredService)))
            {
                var documentExecuter = new EfDocumentExecuter();
                #region MultiExecutionOptions
                var executionOptions = new ExecutionOptions
                {
                    Schema = schema,
                    Query = query,
                    UserContext = new UserContext
                    {
                        DbContext1 = dbContext1,
                        DbContext2 = dbContext2
                    }
                };
                #endregion

                var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
                ObjectApprover.VerifyWithJson(executionResult.Data);
            }
        }
    }

    public MultiContextTests(ITestOutputHelper output) :
        base(output)
    {
    }
}
#region MultiUserContext
public class UserContext
{
    public DbContext1 DbContext1;
    public DbContext2 DbContext2;
}
#endregion