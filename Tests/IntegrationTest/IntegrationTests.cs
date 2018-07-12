using System;
using System.Threading.Tasks;
using EfCore.InMemoryHelpers;
using EfCoreGraphQL;
using GraphQL;
using GraphQL.Types;
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
            dataContext.AddRange(entity1,entity2);
            dataContext.SaveChanges();

            var resolver = BuildResolver(dataContext, query);

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

            if (typeof(ScalarGraphType).IsAssignableFrom(x))
            {
                return Scalar.Build(x);
            }

            throw new Exception($"Could not resolve {x.FullName}");
        });
    }

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}