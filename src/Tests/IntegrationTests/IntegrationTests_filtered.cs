using System.Threading.Tasks;
using VerifyXunit;
using Xunit;
using Filters = GraphQL.EntityFramework.Filters;

public partial class IntegrationTests
{
    [Fact]
    public async Task Child_filtered()
    {
        var query = @"
{
  parentEntitiesFiltered
  {
    property
    children
    {
      property
    }
  }
}";

        FilterParentEntity entity1 = new()
        {
            Property = "Value1"
        };
        FilterChildEntity entity2 = new()
        {
            Property = "Ignore",
            Parent = entity1
        };
        FilterChildEntity entity3 = new()
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        await using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, BuildFilters(), false, entity1, entity2, entity3);
        await Verifier.Verify(result);
    }

    static Filters BuildFilters()
    {
        Filters filters = new();
        filters.Add<FilterParentEntity>((_, item) => item.Property != "Ignore");
        filters.Add<FilterChildEntity>((_, item) => item.Property != "Ignore");
        return filters;
    }

    [Fact]
    public async Task RootList_filtered()
    {
        var query = @"
{
  parentEntitiesFiltered
  {
    property
  }
}";

        FilterParentEntity entity1 = new()
        {
            Property = "Value1"
        };
        FilterParentEntity entity2 = new()
        {
            Property = "Ignore"
        };

        await using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, BuildFilters(), false, entity1, entity2);
        await Verifier.Verify(result);
    }

    [Fact]
    public async Task Root_connectionFiltered()
    {
        var query = @"
{
  parentEntitiesConnectionFiltered {
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
        FilterParentEntity entity1 = new()
        {
            Property = "Value1"
        };
        FilterParentEntity entity2 = new()
        {
            Property = "Ignore"
        };

        await using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, BuildFilters(), false, entity1, entity2);
        await Verifier.Verify(result);
    }

    [Fact(Skip = "Work out why include is not used")]
    public async Task Connection_parent_child_Filtered()
    {
        var query = @"
{
  parentEntitiesConnectionFiltered {
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
        FilterParentEntity entity1 = new()
        {
            Property = "Value1"
        };
        FilterChildEntity entity2 = new()
        {
            Property = "Ignore"
        };
        FilterChildEntity entity3 = new()
        {
            Property = "Value3"
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        await using var database = await sqlInstance.Build();
        var result = await RunQuery(database, query, null, BuildFilters(), false, entity1, entity2, entity3);
        await Verifier.Verify(result);
    }
}