using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

public partial class IntegrationTests :
    XunitApprovalBase
{
    static SqlInstance<IntegrationDbContext> sqlInstance;

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
        GraphTypeTypeRegistry.Register<NamedIdEntity, NamedIdGraph>();
        GraphTypeTypeRegistry.Register<WithMisNamedQueryChildEntity, WithMisNamedQueryChildGraph>();

        sqlInstance = new SqlInstance<IntegrationDbContext>(
            buildTemplate: async dbContext =>
            {
                await dbContext.Database.EnsureCreatedAsync();
                await dbContext.Database.ExecuteSqlRawAsync(
                    @"create view ParentEntityView as
        select Property
        from ParentEntities");
            },
            constructInstance: builder => new IntegrationDbContext(builder.Options));
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };
        var entity3 = new ParentEntity
        {
            Property = "Value3"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Where_with_nullable_properties1()
    {
        var query = "{ withNullableEntities (where: {path: 'Nullable', comparison: 'equal'}){ id } }";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity
        {
            Nullable = 10
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Where_with_nullable_properties2()
    {
        var query = "{ withNullableEntities (where: {path: 'Nullable', comparison: 'equal', value: '10'}){ id } }";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity
        {
            Nullable = 10
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Where_null_comparison_value()
    {
        var query = "{ parentEntities (where: {path: 'Property', comparison: 'equal'}){ id } }";

        var entity1 = new ParentEntity
        {
            Property = null
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entities.ToArray());
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Connection_nested()
    {
        var query = @"
{
  parentEntities {
    id
    childrenConnection(first:2, after:""2"") {
      edges {
        cursor
        node {
          id
        }
      }
	  pageInfo {
		  endCursor
		  hasNextPage
		}
    }
  }
}
";
        var entities = BuildEntities(8);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entities.ToArray());
        ObjectApprover.Verify(result);
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

    [Fact(Skip = "Work out how to eval server side")]
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity2, entity1);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        var inputs = new Inputs(
            new Dictionary<string, object>
            {
                {"value", "value2"}
            });
        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, inputs, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = long.MaxValue
        };
        var entity2 = new CustomTypeEntity
        {
            Property = 3
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Single_NotFound()
    {
        var query = @"
{
  parentEntity(id: ""00000000-0000-0000-0000-000000000001"") {
    property
  }
}";
        using var database = await sqlInstance.Build();
        var error = await Assert.ThrowsAsync<ExecutionError>(() => RunQuery(database, query, null, null));
        ObjectApprover.Verify(error.Message);
    }

    [Fact]
    public async Task Single_Found()
    {
        var query = @"
{
  parentEntity(id: ""00000000-0000-0000-0000-000000000001"") {
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
        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task SingleNullable_NotFound()
    {
        var query = @"
{
  parentEntityNullable(id: ""00000000-0000-0000-0000-000000000001"") {
    property
  }
}";
        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task SingleNullable_Found()
    {
        var query = @"
{
  parentEntityNullable(id: ""00000000-0000-0000-0000-000000000001"") {
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
        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact(Skip = "Work out how to eval server side")]
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Id_string_named()
    {
        var query = @"
query ($entityId: String!)
{
  parentEntities(id:$entityId)
  {
    id
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        var inputs = new Inputs(
            new Dictionary<string, object>
            {
                {"entityId", "00000000-0000-0000-0000-000000000001"}
            });
        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, inputs, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task Id_string()
    {
        var query = @"
query ($id: String!)
{
  parentEntities(id:$id)
  {
    id
  }
}";

        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        var inputs = new Inputs(
            new Dictionary<string, object>
            {
                {"id", "00000000-0000-0000-0000-000000000001"}
            });
        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, inputs, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact]
    public async Task NamedId()
    {
        var query = @"
{
  namedEntities (ids: '00000000-0000-0000-0000-000000000001')
  {
    property
  }
}";

        var entity1 = new NamedIdEntity
        {
            NamedId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new NamedIdEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2);
        ObjectApprover.Verify(result);
    }

    [Fact(Skip = "Work out why include is not used")]
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
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2"
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3"
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5"
        };
        entity4.Children.Add(entity5);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.Verify(result);
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
            Property = "Value"
        };
        var level2 = new Level2Entity
        {
            Level3Entity = level3
        };
        var level1 = new Level1Entity
        {
            Level2Entity = level2
        };

        var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, level1, level2, level3);
        ObjectApprover.Verify(result);
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
            Property = "Value"
        };
        var level2 = new Level2Entity
        {
            Level3Entity = level3
        };
        var level1 = new Level1Entity
        {
            Level2Entity = level2
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, level1, level2, level3);
        ObjectApprover.Verify(result);
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
            Level3Entity = level3a
        };
        var level1a = new Level1Entity
        {
            Level2Entity = level2a
        };

        var level2b = new Level2Entity();
        var level1b = new Level1Entity
        {
            Level2Entity = level2b
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, level1b, level2b, level1a, level2a, level3a);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.Verify(result);
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
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity5 = new ChildEntity
        {
            Property = "Value5"
        };

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity5);
        ObjectApprover.Verify(result);
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

        var entity1 = new WithMisNamedQueryParentEntity();
        var entity2 = new WithMisNamedQueryChildEntity
        {
            Parent = entity1
        };
        var entity3 = new WithMisNamedQueryChildEntity
        {
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new WithMisNamedQueryParentEntity();
        var entity5 = new WithMisNamedQueryChildEntity
        {
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.Verify(result);
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
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = "Value2",
            Parent = entity1
        };
        var entity3 = new ChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var entity4 = new ParentEntity
        {
            Property = "Value4"
        };
        var entity5 = new ChildEntity
        {
            Property = "Value5",
            Parent = entity4
        };
        entity4.Children.Add(entity5);

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, entity1, entity2, entity3, entity4, entity5);
        ObjectApprover.Verify(result);
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

        var parent = new WithManyChildrenEntity();
        var child1 = new Child1Entity
        {
            Parent = parent
        };
        var child2 = new Child2Entity
        {
            Parent = parent
        };
        parent.Child1 = child1;
        parent.Child2 = child2;

        using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, null, parent, child1, child2);
        ObjectApprover.Verify(result);
    }

    static async Task<object> RunQuery(
        SqlDatabase<IntegrationDbContext> database,
        string query,
        Inputs? inputs,
        Filters? filters,
        params object[] entities)
    {
        var dbContext = database.Context;
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton(database.Context);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        return await QueryExecutor.ExecuteQuery(query, services, dbContext, inputs, filters);
    }

    static IEnumerable<Type> GetGraphQlTypes()
    {
        return typeof(IntegrationTests).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract && typeof(GraphType).IsAssignableFrom(x));
    }
}