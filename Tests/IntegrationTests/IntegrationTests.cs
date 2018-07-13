using System;
using System.Threading.Tasks;
using EfCore.InMemoryHelpers;
using EfCoreGraphQL;
using GraphQL;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTests : TestBase
{
    [Fact]
    public async Task Foo()
    {
        var queryString = "{ testEntities { id property } }";

        var entity1 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Wheres()
    {
        var queryString = @"
{ testEntities
  (wheres:
    [
      {path: ""Property"", comparison: ""StartsWith"", value: ""Valu""}
      {path: ""Property"", comparison: ""EndsWith"", value: ""ue3""}
    ]
  )
  {
    property
  }
}";

        var entity1 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        var entity3 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Property = "Value3"
        };

        var result = await RunQuery(queryString, entity1, entity2, entity3);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Where()
    {
        var queryString = "{ testEntities (where: {path: \"Property\", comparison: \"==\", value: \"Value2\"}){ property } }";

        var entity1 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    static async Task<ExecutionResult> RunQuery(string queryString, params object[] entities)
    {
        var query = new Query(x => (MyDataContext) x.UserContext);
        using (var dataContext = InMemoryContextBuilder.Build<MyDataContext>())
        {
            dataContext.AddRange(entities);
            dataContext.SaveChanges();

            var resolver = BuildResolver(dataContext, query);

            var schema = new Schema(resolver);

            var documentExecuter = new DocumentExecuter();

            var executionOptions = new ExecutionOptions
            {
                Schema = schema,
                Query = queryString,
                UserContext = dataContext
            };

            return await documentExecuter.ExecuteAsync(executionOptions).ConfigureAwait(false);
        }
    }

    static FuncDependencyResolver BuildResolver(MyDataContext dataContext, Query query)
    {
        return new FuncDependencyResolver(x =>
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

            if (Scalar.TryGet(x, out var scalar))
            {
                return scalar;
            }

            if (ArgumentGraphTypes.TryGet(x, out var instance))
            {
                return instance;
            }

            throw new Exception($"Could not resolve {x.FullName}");
        });
    }

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}