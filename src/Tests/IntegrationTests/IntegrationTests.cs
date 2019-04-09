using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ObjectApproval;
using Xunit;
using Xunit.Abstractions;

public partial class IntegrationTests :
    TestBase
{
    static IntegrationTests()
    {
        GraphTypeTypeRegistry.Register<FilterChildEntity, FilterChildGraph>();
        GraphTypeTypeRegistry.Register<FilterParentEntity, FilterParentGraph>();
        GraphTypeTypeRegistry.Register<WithManyChildrenEntity, WithManyChildrenGraph>();
        GraphTypeTypeRegistry.Register<CustomTypeEntity, CustomTypeGraph>();
        GraphTypeTypeRegistry.Register<Child1Entity, Child1Graph>();
        GraphTypeTypeRegistry.Register<ChildEntity, ChildGraph>();
        GraphTypeTypeRegistry.Register<ParentEntity, ParentGraph>();
        GraphTypeTypeRegistry.Register<Level1Entity, Level1Graph>();
        GraphTypeTypeRegistry.Register<Level2Entity, Level2Graph>();
        GraphTypeTypeRegistry.Register<Level3Entity, Level3Graph>();
        GraphTypeTypeRegistry.Register<WithMisNamedQueryParentEntity, WithMisNamedQueryParentGraph>();
        GraphTypeTypeRegistry.Register<WithNullableEntity, WithNullableGraph>();
        GraphTypeTypeRegistry.Register<WithMisNamedQueryChildEntity, WithMisNamedQueryChildGraph>();

        using (var dbContext = BuildDbContext())
        {
            dbContext.Database.EnsureCreated();
        }
    }

    public IntegrationTests(ITestOutputHelper output) :
        base(output)
    {
    }

    [Fact]
    public async Task Where_multiple()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_with_nullable_properties1()
    {
        var query = "{ withNullableEntities (where: {path: 'Nullable', comparison: 'equal'}){ id } }";

        var entity1 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
        };
        var entity2 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Nullable = 10
        };

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_with_nullable_properties2()
    {
        var query = "{ withNullableEntities (where: {path: 'Nullable', comparison: 'equal', value: '10'}){ id } }";

        var entity1 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
        };
        var entity2 = new WithNullableEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Nullable = 10
        };

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_null_comparison_value()
    {
        var query = "{ parentEntities (where: {path: 'Property', comparison: 'equal'}){ id } }";

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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Take()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Skip()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Connection_first_page()
    {
        var query = @"
{
  parentEntitiesConnection(first:2, after: '0') {
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

        var result = await RunQuery(query, null, true, null, entities.ToArray());
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
        var query = @"
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

        var result = await RunQuery(query, null, false, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task OrderBy()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity2, entity1);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task OrderByDescending()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Like()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_with_variable()
    {
        var query = @"
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
        var result = await RunQuery(query, inputs, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task CustomType()
    {
        var query = @"
{
  customType
  {
    property
  }
}";

        var entity1 = new CustomTypeEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = long.MaxValue
        };
        var entity2 = new CustomTypeEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = 3
        };

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task SingleParent_Child()
    {
        var query = @"
{
  parentEntity(id: ""00000000-0000-0000-0000-000000000001"") {
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Where_default_comparison()
    {
        var query = @"
{
  parentEntities (where: {path: 'Property', value: 'value2'})
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task In_case_sensitive()
    {
        var query = @"
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

        var result = await RunQuery(query, null, false, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Id()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Id_multiple()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task In()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task In_multiple()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Connection_parent_child()
    {
        var query = @"
{
  parentEntitiesConnection(first:2, after: '0') {
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity4, entity5);

        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Child_parent_with_alias()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Skip_level()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, level1, level2, level3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Multiple_nested()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, level1, level2, level3);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Null_on_nested()
    {
        var query = @"
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
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004")
        };
        var level1b = new Level1Entity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Level2Entity = level2b
        };

        var result = await RunQuery(query, null, true, null, level1b, level2b, level1a, level2a, level3a);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Child_parent()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task With_null_navigation_property()
    {
        var query = @"
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
            Property = "Value5"
        };

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task MisNamedQuery()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Parent_child()
    {
        var query = @"
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

        var result = await RunQuery(query, null, true, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Many_children()
    {
        var query = @"
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
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
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

        var result = await RunQuery(query, null, true, null, parent, child1, child2);
        ObjectApprover.VerifyWithJson(result);
    }

    static async Task<object> RunQuery(string query, Inputs inputs, bool throwForClientEval, GlobalFilters filters, params object[] entities)
    {
        Purge();

        using (var dbContext = BuildDbContext(throwForClientEval))
        {
            dbContext.AddRange(entities);
            dbContext.SaveChanges();
        }

        using (var dbContext = BuildDbContext(throwForClientEval))
        {
            var services = new ServiceCollection();

            services.AddSingleton<Query>();
            foreach (var type in GetGraphQlTypes())
            {
                services.AddSingleton(type);
            }

            return await QueryExecutor.ExecuteQuery(query, services, dbContext, inputs, filters);
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
        using (var dbContext = BuildDbContext())
        {
            Purge(dbContext.CustomTypeEntities);
            Purge(dbContext.Level1Entities);
            Purge(dbContext.Level2Entities);
            Purge(dbContext.Level3Entities);
            Purge(dbContext.ChildEntities);
            Purge(dbContext.ParentEntities);
            Purge(dbContext.WithMisNamedQueryChildEntities);
            Purge(dbContext.WithMisNamedQueryParentEntities);
            Purge(dbContext.WithNullableEntities);
            Purge(dbContext.WithManyChildrenEntities);
            Purge(dbContext.FilterParentEntities);
            Purge(dbContext.FilterChildEntities);
            Purge(dbContext.Child1Entities);
            Purge(dbContext.Child2Entities);
            dbContext.SaveChanges();
        }
    }

    static void Purge<T>(DbSet<T> dbSet)
        where T : class
    {
        dbSet.RemoveRange(dbSet);
    }

    static MyDbContext BuildDbContext(bool throwForClientEval = true)
    {
        var builder = new DbContextOptionsBuilder<MyDbContext>();
        builder.UseSqlServer(Connection.ConnectionString);
        if (throwForClientEval)
        {
            builder.ConfigureWarnings(x => x.Throw(RelationalEventId.QueryClientEvaluationWarning));
        }

        return new MyDbContext(builder.Options);
    }
}