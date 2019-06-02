using System.Threading.Tasks;
using GraphQL.EntityFramework;
using ObjectApproval;
using Xunit;

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

        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "Ignore",
            Parent = entity1
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Value3",
            Parent = entity1
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);
        var result = await RunQuery(await sqlInstance.Build(), query, null, BuildFilters(), entity1, entity2, entity3);
        ObjectApprover.VerifyWithJson(result);
    }

    static GlobalFilters BuildFilters()
    {
        var filters = new GlobalFilters();
        filters.Add<FilterParentEntity>((context, item) => item.Property != "Ignore");
        filters.Add<FilterChildEntity>((context, item) => item.Property != "Ignore");
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

        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterParentEntity
        {
            Property = "Ignore"
        };

        var result = await RunQuery(await sqlInstance.Build(), query, null, BuildFilters(), entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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
        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterParentEntity
        {
            Property = "Ignore"
        };

        var result = await RunQuery(await sqlInstance.Build(), query, null, BuildFilters(), entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
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
        var entity1 = new FilterParentEntity
        {
            Property = "Value1"
        };
        var entity2 = new FilterChildEntity
        {
            Property = "Ignore"
        };
        var entity3 = new FilterChildEntity
        {
            Property = "Value3"
        };
        entity1.Children.Add(entity2);
        entity1.Children.Add(entity3);

        var result = await RunQuery(await sqlInstance.Build(), query, null, BuildFilters(), entity1, entity2, entity3);

        ObjectApprover.VerifyWithJson(result);
    }
}