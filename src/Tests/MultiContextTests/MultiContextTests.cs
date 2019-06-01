using System.Threading.Tasks;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class MultiContextTests:
    XunitLoggingBase
{
    static MultiContextTests()
    {
        GraphTypeTypeRegistry.Register<Entity1, Entity1Graph>();
        GraphTypeTypeRegistry.Register<Entity2, Entity2Graph>();

        using (var dbContext = BuildDbContext1())
        {
            var database = dbContext.Database;
            var script = database.GenerateCreateScript().Replace("GO","");
            try
            {
                database.ExecuteSqlCommand(script);
            }
            catch
            {
            }
        }
        using (var dbContext = BuildDbContext2())
        {
            var database = dbContext.Database;
            var script = database.GenerateCreateScript().Replace("GO","");
            try
            {
                database.ExecuteSqlCommand(script);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task Run()
    {
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

        Purge();

        using (var dbContext = BuildDbContext1())
        {
            dbContext.AddRange(entity1);
            dbContext.SaveChanges();
        }

        using (var dbContext = BuildDbContext2())
        {
            dbContext.AddRange(entity2);
            dbContext.SaveChanges();
        }

        using (var dbContext1 = BuildDbContext1())
        using (var dbContext2 = BuildDbContext2())
        {
            var services = new ServiceCollection();

            services.AddSingleton<MultiContextQuery>();
            services.AddSingleton(typeof(Entity1Graph));
            services.AddSingleton(typeof(Entity2Graph));

            EfGraphQLConventions.RegisterInContainer(services, dbContext1);
            EfGraphQLConventions.RegisterInContainer(services, dbContext2);
            using (var provider = services.BuildServiceProvider())
            using (var schema = new MultiContextSchema(new FuncDependencyResolver(provider.GetRequiredService)))
            {
                var documentExecuter = new EfDocumentExecuter();

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

                var executionResult = await documentExecuter.ExecuteWithErrorCheck(executionOptions);
                ObjectApprover.VerifyWithJson(executionResult.Data);
            }
        }
    }

    static void Purge()
    {
        using (var dbContext = BuildDbContext1())
        {
            Purge(dbContext.Entities);
            dbContext.SaveChanges();
        }
        using (var dbContext = BuildDbContext2())
        {
            Purge(dbContext.Entities);
            dbContext.SaveChanges();
        }
    }

    static void Purge<T>(DbSet<T> dbSet)
        where T : class
    {
        dbSet.RemoveRange(dbSet);
    }

    static DbContext1 BuildDbContext1()
    {
        var builder = new DbContextOptionsBuilder<DbContext1>();
        builder.UseSqlServer(Connection.ConnectionString);
        return new DbContext1(builder.Options);
    }

    static DbContext2 BuildDbContext2()
    {
        var builder = new DbContextOptionsBuilder<DbContext2>();
        builder.UseSqlServer(Connection.ConnectionString);
        return new DbContext2(builder.Options);
    }

    public MultiContextTests(ITestOutputHelper output) :
        base(output)
    {
    }
}

public class UserContext
{
    public DbContext1 DbContext1;
    public DbContext2 DbContext2;
}