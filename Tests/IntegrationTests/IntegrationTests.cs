using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.EntityFramework;
using GraphQL;
using GraphQL.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public class IntegrationTests : TestBase
{
    static IntegrationTests()
    {
        using (var dataContext = BuildDataContext())
        {
       //     dataContext.Database.EnsureDeleted();
            dataContext.Database.EnsureCreated();
        }
    }

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Foo()
    {
        var queryString = "{ parentEntities { id property } }";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities
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

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        var entity3 = new ParentEntity
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
        var queryString = "{ parentEntities (where: {path: 'Nullable', comparison: '=='}){ id } }";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = null
        };
        var entity2 = new ParentEntity
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
        var queryString = "{ parentEntities (where: {path: 'Nullable', comparison: '==', value: '10'}){ id } }";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = null
        };
        var entity2 = new ParentEntity
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
        var queryString = "{ parentEntities (where: {path: 'Property', comparison: '=='}){ id } }";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = null
        };
        var entity2 = new ParentEntity
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
  parentEntities (take: 1)
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities (skip: 1)
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntitiesConnection(first:2, after: '1') {
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

    static IEnumerable<ParentEntity> BuildEntities(uint length)
    {
        for (var index = 0; index < length; index++)
        {
            yield return new ParentEntity
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
  parentEntities (where: {path: 'Property', comparison: '==', value: 'Value2', case: 'Ordinal' })
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task OrderBy()
    {
        var queryString = @"
{
  parentEntities (orderBy: {path: 'Property'})
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };

        var result = await RunQuery(queryString, entity2, entity1);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task OrderByDescending()
    {
        var queryString = @"
{
  parentEntities (orderBy: {path: 'Property', descending: true})
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities (where: {path: 'Property', comparison: '==', value: 'value2'})
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities (where: {path: 'Property', comparison: 'In', value: 'Value2', case: 'Ordinal' })
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities (ids: '00000000-0000-0000-0000-000000000001')
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities
  (ids: ['00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002'])
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        var entity3 = new ParentEntity
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
  parentEntities (where: {path: 'Property', comparison: 'In', value: 'value2'})
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
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
  parentEntities
  (where: {path: 'Property', comparison: 'In', value: ['Value1', 'Value2']})
  {
    property
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };

        var result = await RunQuery(queryString, entity1, entity2);
        ObjectApprover.VerifyWithJson(result.Data);
    }


    [Fact]
    public async Task Connection_parent_child()
    {
        var queryString = @"
{
  parentEntitiesConnection(first:2, after: '1') {
    totalCount
    edges {
      cursor
      node {
        property
        children
        {
          property
        }
      }
    }
    items {
      property
      children
      {
        property
      }
    }
  }
}

";
        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };
        var entity3 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Property = "Value3"
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Property = "Value5"
        };
        entity4.Children.Add(entity5);

        var result = await RunQuery(queryString, entity1, entity2, entity3, entity4, entity5);

        ObjectApprover.VerifyWithJson(result.Data);
    }

    [Fact]
    public async Task Parent_child()
    {
        var queryString = @"
{
  parentEntities
  {
    property
    children
    {
      property
    }
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        var result = await RunQuery(queryString, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result.Data);
    }

    static async Task<ExecutionResult> RunQuery(string queryString, params object[] entities)
    {
        Purge();

        using (var dataContext = BuildDataContext())
        {
            dataContext.AddRange(entities);
            dataContext.SaveChanges();
        }

        queryString = queryString.Replace("'", "\"");

        using (var dataContext = BuildDataContext())
        {
            var services = new ServiceCollection();

            services.AddTransient(typeof(ConnectionType<>));
            services.AddTransient(typeof(EdgeType<>));
            services.AddTransient<PageInfoType>();
            services.AddSingleton(dataContext);
            services.AddSingleton<Query>();
            services.AddSingleton<ChildGraph>();
            services.AddSingleton<ParentGraph>();

            EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
            EfGraphQLConventions.RegisterInContainer(services, dataContext);
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

    static void Purge()
    {
        using (var dataContext = BuildDataContext())
        {
            dataContext.ChildEntities.RemoveRange(dataContext.ChildEntities);
            dataContext.ParentEntities.RemoveRange(dataContext.ParentEntities);
            dataContext.SaveChanges();
        }
    }

    static MyDataContext BuildDataContext()
    {
        var builder = new DbContextOptionsBuilder<MyDataContext>();
        var connection = @"Data Source=.\SQLExpress;Database=GraphQLEntityFrameworkTests; Integrated Security=True;Max Pool Size=100";
        builder.UseSqlServer(connection);
        return new MyDataContext(builder.Options);
    }
}