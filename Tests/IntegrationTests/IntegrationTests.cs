using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public partial class IntegrationTests : TestBase
{
    static IntegrationTests()
    {
        using (var dataContext = BuildDataContext())
        {
            dataContext.Database.EnsureCreated();
        }
    }

    public IntegrationTests(ITestOutputHelper output) : base(output)
    {
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

        var result = await RunQuery(queryString, null, entity1, entity2, entity3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_with_nullable_properties1()
    {
        var queryString = "{ withNullableEntities (where: {path: 'Nullable', comparison: 'equal'}){ id } }";

        var entity1 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        };
        var entity2 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Nullable = 10
        };

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_with_nullable_properties2()
    {
        var queryString = "{ withNullableEntities (where: {path: 'Nullable', comparison: 'equal', value: '10'}){ id } }";

        var entity1 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        };
        var entity2 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Nullable = 10
        };

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_null_comparison_value()
    {
        var queryString = "{ parentEntities (where: {path: 'Property', comparison: 'equal'}){ id } }";

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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entities.ToArray());
        ObjectApprover.VerifyWithJson(result);
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
  parentEntities (where: {path: 'Property', comparison: 'equal', value: 'Value2', case: 'Ordinal' })
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity2, entity1);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Like()
    {
        var queryString = @"
{
  parentEntities (where: {path: 'Property', comparison: 'Like', value: 'value2'})
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_with_variable()
    {
        var queryString = @"
query ($value: String!)
{
  parentEntities (where: {path: 'Property', comparison: 'equal', value: [$value]})
  {
    property
  }
}
";

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

        var inputs = new Inputs(new Dictionary<string, object>
        {
            {"value", "value2"}
        });
        var result = await RunQuery(queryString, inputs, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where()
    {
        var queryString = @"
{
  parentEntities (where: {path: 'Property', comparison: 'equal', value: 'value2'})
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2, entity3);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2, entity3, entity4, entity5);

        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Child_parent_with_alias()
    {
        var queryString = @"
{
  childEntities
  {
    parentAlias
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

        var result = await RunQuery(queryString, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Skip_level()
    {
        var queryString = @"
{
  skipLevel
  {
    level3Entity
    {
      property
    }
  }
}";

        var level3 = new Level3Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Property = "Value"
        };
        var level2 = new Level2Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Level3Entity = level3
        };
        var level1 = new Level1Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Level2Entity = level2
        };

        var result = await RunQuery(queryString, null, level1, level2, level3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Multiple_nested()
    {
        var queryString = @"
{
  level1Entities
  {
    level2Entity
    {
      level3Entity
      {
        property
      }
    }
  }
}";

        var level3 = new Level3Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Property = "Value"
        };
        var level2 = new Level2Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Level3Entity = level3
        };
        var level1 = new Level1Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Level2Entity = level2
        };

        var result = await RunQuery(queryString, null, level1, level2, level3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Null_on_nested()
    {
        var queryString = @"
{
  level1Entities(where: {path: 'Level2Entity.Level3EntityId', comparison: 'equal', value: '00000000-0000-0000-0000-000000000003'})
  {
    level2Entity
    {
      level3Entity
      {
        property
      }
    }
  }
}";

        var level3a = new Level3Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Property = "Valuea"
        };
        var level2a = new Level2Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Level3Entity = level3a
        };
        var level1a = new Level1Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Level2Entity = level2a
        };

        var level2b = new Level2Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
        };
        var level1b = new Level1Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Level2Entity = level2b
        };

        var result = await RunQuery(queryString, null, level1b, level2b, level1a, level2a, level3a);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Child_parent()
    {
        var queryString = @"
{
  childEntities
  {
    property
    parent
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

        var result = await RunQuery(queryString, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task With_null_navigation_property()
    {
        var queryString = @"
{
  childEntities(where: {path: 'ParentId', comparison: 'equal', value: '00000000-0000-0000-0000-000000000001'})
  {
    property
    parent
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
        var entity5 = new ChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Property = "Value5",
        };

        var result = await RunQuery(queryString, null, entity1, entity2, entity3, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task MisNamedQuery()
    {
        var queryString = @"
{
  misNamed
  {
    misNamedChildren
    {
      id
    }
  }
}";

        var entity1 = new WithMisNamedQueryParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
        };
        var entity2 = new WithMisNamedQueryChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Parent = entity1
        };
        var entity3 = new WithMisNamedQueryChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new WithMisNamedQueryParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004")
        };
        var entity5 = new WithMisNamedQueryChildEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        var result = await RunQuery(queryString, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
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

        var result = await RunQuery(queryString, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Many_children()
    {
        var queryString = @"
{
  manyChildren
  {
    child1
    {
      id
    }
  }
}";

        var parent = new WithManyChildrenEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        };
        var child1 = new Child1Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Parent = parent
        };
        var child2 = new Child2Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Parent = parent
        };
        parent.Child1 = child1;
        parent.Child2 = child2;

        var result = await RunQuery(queryString, null, parent, child1, child2);
        ObjectApprover.VerifyWithJson(result);
    }

    static async Task<object> RunQuery(string queryString, Inputs inputs, params object[] entities)
    {
        Purge();

        using (var dataContext = BuildDataContext())
        {
            dataContext.AddRange(entities);
            dataContext.SaveChanges();
        }

        using (var dataContext = BuildDataContext())
        {
            var services = new ServiceCollection();

            services.AddSingleton<Query>();
            foreach (var type in GetGraphQlTypes())
            {
                services.AddSingleton(type);
            }

            return await QueryExecutor.ExecuteQuery(queryString, services, dataContext, inputs);
        }
    }

    static IEnumerable<Type> GetGraphQlTypes()
    {
        return typeof(IntegrationTests).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract && typeof(GraphType).IsAssignableFrom(x));
    }

    static void Purge()
    {
        using (var dataContext = BuildDataContext())
        {
            Purge(dataContext.Level1Entities);
            Purge(dataContext.Level2Entities);
            Purge(dataContext.Level3Entities);
            Purge(dataContext.ChildEntities);
            Purge(dataContext.ParentEntities);
            Purge(dataContext.WithMisNamedQueryChildEntities);
            Purge(dataContext.WithMisNamedQueryParentEntities);
            Purge(dataContext.WithNullableEntities);
            Purge(dataContext.WithManyChildrenEntities);
            Purge(dataContext.FilterParentEntities);
            Purge(dataContext.FilterChildEntities);
            Purge(dataContext.Child1Entities);
            Purge(dataContext.Child2Entities);
            dataContext.SaveChanges();
        }
    }

    static void Purge<T>(DbSet<T> dbSet) where T : class
    {
        dbSet.RemoveRange(dbSet);
    }

    static MyDataContext BuildDataContext()
    {
        var builder = new DbContextOptionsBuilder<MyDataContext>();
        builder.UseSqlServer(Connection.ConnectionString);
        return new MyDataContext(builder.Options);
    }
}