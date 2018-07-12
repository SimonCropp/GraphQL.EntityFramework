using System;
using System.Threading.Tasks;
using EfCore.InMemoryHelpers;
using EfCoreGraphQL;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTests : TestBase
{
    [Fact]
    public async Task Foo()
    {
        var query = new Query(x => (MyDataContext) x.UserContext);
        using (var dataContext = InMemoryContextBuilder.Build<MyDataContext>())
        {
            var entity = new TestEntity
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Property = "Value"
            };
            dataContext.AddRange(entity);
            dataContext.SaveChanges();

            var resolver = new FuncDependencyResolver(x =>
            {
                if (x == typeof(MyDataContext))
                {
                    return dataContext;
                }

                if (x == typeof(Query))
                {
                    return query;
                }

                if (x == typeof(TestEntityType))
                {
                    return new TestEntityType();
                }

                if (typeof(ScalarGraphType).IsAssignableFrom(x))
                {
                    return Scalar.Build(x);
                }

                throw new Exception($"Could not resolve {x.FullName}");
            });

            var schema = new Schema(resolver);

            var documentExecuter = new DocumentExecuter();

            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = "{ testEntities { id property } }",
                UserContext = dataContext
            };

            var result = await documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);
            ObjectApprover.VerifyWithJson(result.Data);
        }
    }

    public class Schema : GraphQL.Types.Schema
    {
        public Schema(IDependencyResolver resolver) :
            base(resolver)
        {
            Query = resolver.Resolve<Query>();
        }
    }

    public class MyDataContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public MyDataContext(DbContextOptions options) :
            base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>();
        }
    }

    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Property { get; set; }
    }

    public class Query : ObjectGraphType
    {
        public Query(ResolveDataContext<MyDataContext> resolveDataContext)
        {
            Field<ListGraphType<TestEntityType>>(
                "testEntities",
                resolve: context =>
                {
                    var dataContext = resolveDataContext(context);
                    return dataContext.TestEntities.ToListAsync();
                });
        }
    }

    public class TestEntityType : ObjectGraphType<TestEntity>
    {
        public TestEntityType()
        {
            Field(x => x.Id);
            Field(x => x.Property);
        }
    }

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}