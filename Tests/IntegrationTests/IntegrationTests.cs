using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfCore.InMemoryHelpers;
using GraphQL.EntityFramework;
using GraphQL;
using GraphQL.Types.Relay;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task Where_multiple()
    {
        var queryString = @"
{
  testEntities
  (where:
    [
      {path: 'Property', comparison: 'startsWith"", value: 'Valu'}
      {path: 'Property', comparison: 'endsWith"", value: 'ue3'}
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
    public async Task Where_with_nullable_properties1()
    {
        var queryString = "{ testEntities (where: {path: 'Nullable', comparison: '=='}){ id } }";

        var entity1 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = null
        };
        var entity2 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Nullable = 10
        };

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Where_with_nullable_properties2()
    {
        var queryString = "{ testEntities (where: {path: 'Nullable', comparison: '==', value: '10'}){ id } }";

        var entity1 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = null
        };
        var entity2 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Nullable = 10
        };

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Where_null_comparison_value()
    {
        var queryString = "{ testEntities (where: {path: 'Property', comparison: '=='}){ id } }";

        var entity1 = new TestEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = null
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
    public async Task Take()
    {
        var queryString = @"
{
  testEntities (take: 1)
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Skip()
    {
        var queryString = @"
{
  testEntities (skip: 1)
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Connection_first_page()
    {
        var queryString = @"
{
  testEntitiesConnection(first:2, after: '1') {
    totalCount
    edges {
      cursor
      node {
        property
      }
    }
    items {
      property
    }
  }
}

";
        var entities = BuildEntities(8);

        var result = await RunQuery(queryString, entities.ToArray());
        ObjectApprover.VerifyWithJson(result.Data);
    }

    static IEnumerable<TestEntity> BuildEntities(uint length)
    {
        for (var index = 0; index < length; index++)
        {
            yield return new TestEntity
            {
                Id = Guid.Parse("00000000-0000-0000-0000-00000000000" + index),
                Property = "Value" + index
            };
        }
    }

    [Fact]
    public async Task Where_case_sensitive()
    {
        var queryString = @"
{
  testEntities (where: {path: 'Property', comparison: '==', value: 'Value2', case: 'Ordinal' })
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Where()
    {
        var queryString = @"
{
  testEntities (where: {path: 'Property', comparison: '==', value: 'value2'})
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task In_case_sensitive()
    {
        var queryString = @"
{
  testEntities (where: {path: 'Property', comparison: 'In', value: 'Value2', case: 'Ordinal' })
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Id()
    {
        var queryString = @"
{
  testEntities (ids: '00000000-0000-0000-0000-000000000001')
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Id_multiple()
    {
        var queryString = @"
{
  testEntities
  (ids: ['00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002'])
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
    public async Task In()
    {
        var queryString = @"
{
  testEntities (where: {path: 'Property', comparison: 'In', value: 'value2'})
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task In_multiple()
    {
        var queryString = @"
{
  testEntities
  (where: {path: 'Property', comparison: 'In', value: ['Value1', 'Value2']})
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

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    static async Task<ExecutionResult> RunQuery(string queryString, params TestEntity[] entities)
    {
        queryString = queryString.Replace("'", "\"");

        var query = new Query();
        using (var dataContext = InMemoryContextBuilder.Build<MyDataContext>())
        {
            dataContext.AddRange(entities);
            dataContext.SaveChanges();
            var services = new ServiceCollection();

            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddTransient<PageInfoType>();
            services.AddSingleton(dataContext);
            services.AddSingleton(query);
            services.AddSingleton<TestEntityGraph>();

            EfCoreGraphQLConventions.RegisterInContainer(services);
            using (var provider = services.BuildServiceProvider())
            using (var schema = new Schema(new FuncDependencyResolver(provider.GetRequiredService)))
            {
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
    }

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
    }
}