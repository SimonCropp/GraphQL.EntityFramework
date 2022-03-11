using EfLocalDb;
using GraphQL;
using GraphQL.EntityFramework;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Filters = GraphQL.EntityFramework.Filters;

[UsesVerify]
public partial class IntegrationTests
{
    static SqlInstance<IntegrationDbContext> sqlInstance;

    static IntegrationTests() =>
        sqlInstance = new(
            buildTemplate: async data =>
            {
                await data.Database.EnsureCreatedAsync();
                await data.Database.ExecuteSqlRawAsync(
                    @"create view ParentEntityView as
        select Property
        from ParentEntities");
            },
            constructInstance: builder =>
            {
                builder.ConfigureWarnings(x => x.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
                return new(builder.Options);
            });

    [Fact]
    public async Task SchemaPrint()
    {
        await using var database = await sqlInstance.Build();
        var dbContext = database.Context;
        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton<Mutation>();
        services.AddSingleton(database.Context);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        EfGraphQLConventions.RegisterInContainer(services, _ => dbContext, dbContext.Model);
        EfGraphQLConventions.RegisterConnectionTypesInContainer(services);
        await using var provider = services.BuildServiceProvider();
        using var schema = new Schema(provider);

        var printer = new SchemaPrinter(schema);
        var print = printer.Print();
        await Verify(print);
    }

    [Fact]
    public async Task LogQuery()
    {
        string? queryText = null;
        QueryLogger.Enable(s => queryText = s);
        var query = @"
{
  parentEntities
  (where:
    [
      {path: 'Property', comparison: startsWith, value: 'Valu'}
      {path: 'Property', comparison: endsWith, value: 'ue3'}
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3 });
        Assert.NotNull(queryText);
    }

    [Fact]
    public async Task Where_multiple()
    {
        var query = @"
{
  parentEntities
  (where:
    [
      {path: 'Property', comparison: startsWith, value: 'Valu'}
      {path: 'Property', comparison: endsWith, value: 'ue3'}
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3 });
    }

    [Fact]
    public async Task Where_with_nullable_properties1()
    {
        var query = "{ withNullableEntities (where: {path: 'Nullable', comparison: equal}){ id } }";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity
        {
            Nullable = 10
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task Where_with_nullable_properties2()
    {
        var query = "{ withNullableEntities (where: {path: 'Nullable', comparison: equal, value: '10'}){ id } }";

        var entity1 = new WithNullableEntity();
        var entity2 = new WithNullableEntity
        {
            Nullable = 10
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task Where_null_comparison_value()
    {
        var query = "{ parentEntities (where: {path: 'Property', comparison: equal}){ id } }";

        var entity1 = new ParentEntity
        {
            Property = null
        };
        var entity2 = new ParentEntity
        {
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task Take()
    {
        var query = @"
{
  parentEntities (take: 1, orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task Skip()
    {
        var query = @"
{
  parentEntities (skip: 1, orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    [Fact]
    public async Task Connection_page_back()
    {
        var query = @"
{
  parentEntitiesConnection(last:2, before: '2') {
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());

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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, entities.ToArray());
    }

    static IEnumerable<ParentEntity> BuildEntities(uint length)
    {
        for (var index = 0; index < length; index++)
        {
            yield return new()
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
  parentEntities (where: {path: 'Property', comparison: equal, value: 'Value2', case: 'Ordinal' })
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity2, entity1 });

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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task Like()
    {
        var query = @"
{
  parentEntities (where: {path: 'Property', comparison: like, value: 'value2'})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task Where_with_variable()
    {
        var query = @"
query ($value: String!)
{
  parentEntities (where: {path: 'Property', comparison: equal, value: [$value]})
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

        var inputs = new Inputs(new Dictionary<string, object?> { { "value", "value2" } });
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, inputs, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task CustomType()
    {
        var query = @"
{
  customType (orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { });
    }

    [Fact]
    public async Task Single_Found_NoTracking()
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
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, true, new object[] { entity1, entity2 });
    }


    [Fact]
    public async Task Single_Found_Large_Text_NoAsync()
    {
        var query = @"
{
  parentEntity(id: ""00000000-0000-0000-0000-000000000001"") {
id
  }
}";
        var largeString = new StringBuilder().Append('a', 3 * 1024 * 1024);
        var entity1 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = largeString.ToString()
        };
        var entity2 = new ParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value2"
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 }, true);
    }

    [Fact]
    public async Task Owned()
    {
        var query = @"
{
  ownedParent(id: ""00000000-0000-0000-0000-000000000001"") {
    property
    child1 {
      property
    }
  }
}";
        var entity1 = new OwnedParent
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Parent value",
            Child1 = new() { Property = "Value1" },
            Child2 = new() { Property = "Value2" }
        };
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1 });
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
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { });
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
        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task SingleParent_Child_mutation()
    {
        var query = @"
mutation {
  parentEntityMutation(id: ""00000000-0000-0000-0000-000000000001"") {
    property
    children(orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact]
    public async Task SingleParent_Child()
    {
        var query = @"
{
  parentEntity(id: ""00000000-0000-0000-0000-000000000001"") {
    property
    children(orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact]
    public async Task SingleParent_Child_WithFragment()
    {
        var query = @"
{
  parentEntity(id: ""00000000-0000-0000-0000-000000000001"") {
    ...parentEntityFields
  }
}
fragment parentEntityFields on Parent {
  property
  children(orderBy: {path: ""property""})
  {
    ...childEntityFields
  }
}
fragment childEntityFields on Child {
  property
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact]
    public async Task Where()
    {
        var query = @"
{
  parentEntities (where: {path: 'Property', comparison: equal, value: 'value2'})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact(Skip = "Work out how to eval server side")]
    public async Task In_case_sensitive()
    {
        var query = @"
{
  parentEntities (where: {path: 'Property', comparison: in, value: 'Value2', case: 'Ordinal' })
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3 });
    }

    [Fact]
    public async Task In()
    {
        var query = @"
{
  parentEntities (where: {path: 'Property', comparison: in, value: 'value2'})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
    }

    [Fact]
    public async Task In_multiple()
    {
        var query = @"
{
  parentEntities
  (where: {path: 'Property', comparison: in, value: ['Value1', 'Value2']}, orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact]
    public async Task Child_parent_with_alias()
    {
        var query = @"
{
  childEntities (orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact(Skip = "TODO")]
    public async Task Multiple_nested_AddQueryField()
    {
        var query = @"
{
  queryFieldWithInclude
  {
    includeNonQueryableB
    {
      id
    }
  }
}";
        var level2 = new IncludeNonQueryableA();
        var level1 = new IncludeNonQueryableB
        {
            IncludeNonQueryableA = level2,
        };
        level1.IncludeNonQueryableA = level2;

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { level1, level2 });
    }

    [Fact(Skip = "fix order")]
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
        await RunQuery(database, query, null, null, false, new object[] { level1, level2, level3 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { level1, level2, level3 });
    }

    [Fact]
    public async Task Null_on_nested()
    {
        var query = @"
{
  level1Entities(where: {path: 'Level2Entity.Level3EntityId', comparison: equal, value: '00000000-0000-0000-0000-000000000003'})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { level1b, level2b, level1a, level2a, level3a });
    }

    [Fact]
    public async Task Query_Cyclic()
    {
        var query = @"
{
  childEntities (orderBy: {path: ""property""})
  {
    property
    parent
    {
      property
      children
      {
        property
        parent
        {
          property
        }
      }
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact]
    public async Task Query_NoTracking()
    {
        var query = @"
{
  childEntities (orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, true, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }


    [Fact]
    public async Task Query_Large_Text_NoAsync()
    {
        var query = @"
{
  childEntities (orderBy: {path: ""property""})
  {
    parent
    {
      property
    }
  }
}";
        var largeString = new StringBuilder().Append('a', 3 * 1024 * 1024);
        var entity1 = new ParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new ChildEntity
        {
            Property = largeString.ToString(),
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 }, true);
    }

    [Fact]
    public async Task Child_parent()
    {
        var query = @"
{
  childEntities (orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact]
    public async Task With_null_navigation_property()
    {
        var query = @"
{
  childEntities(where: {path: 'ParentId', comparison: equal, value: '00000000-0000-0000-0000-000000000001'}, orderBy: {path: ""property""})
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity5 });
    }

    [Fact(Skip = "fix order")]
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
    }

    [Fact(Skip = "fix order")]
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { entity1, entity2, entity3, entity4, entity5 });
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

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { parent, child1, child2 });
    }

    [Fact(Skip = "Broke with gql 4")]
    public async Task InheritedEntityInterface()
    {
        var query = @"
{
  interfaceGraphConnection {
    items {
      ...inheritedEntityFields
    }
  }
}
fragment inheritedEntityFields on Interface {
  property
  childrenFromInterface(orderBy: {path: ""property""})
  {
    items {
      ...childEntityFields
    }
  }
  ... on DerivedWithNavigation {
    childrenFromDerived(orderBy: {path: ""property""})
    {
      items {
        ...childEntityFields
      }
    }
  }
}
fragment childEntityFields on DerivedChild {
  property
}";

        var derivedEntity1 = new DerivedEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var childEntity1 = new DerivedChildEntity
        {
            Property = "Value2",
            Parent = derivedEntity1
        };
        var childEntity2 = new DerivedChildEntity
        {
            Property = "Value3",
            Parent = derivedEntity1
        };
        derivedEntity1.ChildrenFromBase.Add(childEntity1);
        derivedEntity1.ChildrenFromBase.Add(childEntity2);

        var derivedEntity2 = new DerivedWithNavigationEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Value4"
        };
        var childEntity3 = new DerivedChildEntity
        {
            Property = "Value5",
            Parent = derivedEntity2
        };
        var childEntity4 = new DerivedChildEntity
        {
            Property = "Value6",
            TypedParent = derivedEntity2
        };
        derivedEntity2.ChildrenFromBase.Add(childEntity3);
        derivedEntity2.Children.Add(childEntity4);

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { derivedEntity1, childEntity1, childEntity2, derivedEntity2, childEntity3, childEntity4 });
    }

    [Fact]
    public async Task ManyToManyRightWhereAndInclude()
    {
        var query = @"
{
  manyToManyLeftEntities (where: {path: 'rights[rightName]', comparison: equal, value: ""Right2""})
  {
    leftName
    rights
    {
      rightName
    }
  }
}";

        var middle11 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left1Id",
                LeftName = "Left1"
            },
            ManyToManyRightEntity = new()
            {
                Id = "Right1Id",
                RightName = "Right1"
            }
        };

        var middle12 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = middle11.ManyToManyLeftEntity,
            ManyToManyRightEntity = new()
            {
                Id = "Right2Id",
                RightName = "Right2"
            }
        };

        var middle21 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left2Id",
                LeftName = "Left2"
            },
            ManyToManyRightEntity = middle11.ManyToManyRightEntity
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { middle11, middle12, middle21 });
    }

    [Fact]
    public async Task ManyToManyLeftWhereAndInclude()
    {
        var query = @"
{
  manyToManyRightEntities (where: {path: 'lefts[leftName]', comparison: equal, value: ""Left2""})
  {
    rightName
    lefts
    {
      leftName
    }
  }
}";

        var middle11 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left1Id",
                LeftName = "Left1"
            },
            ManyToManyRightEntity = new()
            {
                Id = "Right1Id",
                RightName = "Right1"
            }
        };

        var middle12 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = middle11.ManyToManyLeftEntity,
            ManyToManyRightEntity = new()
            {
                Id = "Right2Id",
                RightName = "Right2"
            }
        };

        var middle21 = new ManyToManyMiddleEntity
        {
            ManyToManyLeftEntity = new()
            {
                Id = "Left2Id",
                LeftName = "Left2"
            },
            ManyToManyRightEntity = middle11.ManyToManyRightEntity
        };

        await using var database = await sqlInstance.Build();
        await RunQuery(database, query, null, null, false, new object[] { middle11, middle12, middle21 });
    }

    static async Task RunQuery(
        SqlDatabase<IntegrationDbContext> database,
        string query,
        Inputs? inputs,
        Filters? filters,
        bool disableTracking,
        object[] entities,
        bool disableAsync = false,
        [CallerFilePath] string sourceFile = "")
    {
        var dbContext = database.Context;
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
        var services = new ServiceCollection();
        services.AddSingleton<Query>();
        services.AddSingleton<Mutation>();
        services.AddSingleton(database.Context);
        foreach (var type in GetGraphQlTypes())
        {
            services.AddSingleton(type);
        }

        await using var context = database.NewDbContext();
        SqlRecording.StartRecording();
        try
        {
            var result = await QueryExecutor.ExecuteQuery(query, services, context, inputs, filters, disableTracking, disableAsync);
            await Verify(result, sourceFile: sourceFile).ScrubInlineGuids();
        }
        catch (ExecutionError executionError)
        {
            await Verify(executionError.Message, sourceFile: sourceFile);
        }
    }

    static IEnumerable<Type> GetGraphQlTypes() =>
        typeof(IntegrationTests).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract && typeof(GraphType).IsAssignableFrom(x));
}