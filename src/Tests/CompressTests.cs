using ApprovalTests;
using GraphQL.EntityFramework;
using Xunit;

public class CompressTests
{
    [Fact]
    public void Simple()
    {
        var query = @"
query ($id: String!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        Approvals.Verify(Compress.Query(query));
    }
}