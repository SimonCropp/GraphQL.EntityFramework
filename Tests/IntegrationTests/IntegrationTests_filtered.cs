using System;
using System.Threading.Tasks;
using ObjectApproval;
using Xunit;

public partial class IntegrationTests
{
    [Fact]
    public async Task RootList_filtered()
    {
        var queryString = @"
{
  parentEntitiesFiltered
  {
    property
  }
}";

        var entity1 = new FilterParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new FilterParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Ignore"
        };

        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }

    [Fact]
    public async Task Root_connectionFiltered()
    {
        var queryString = @"
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
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Property = "Value1"
        };
        var entity2 = new FilterParentEntity
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Property = "Ignore"
        };


        var result = await RunQuery(queryString, null, entity1, entity2);
        ObjectApprover.VerifyWithJson(result);
    }
}