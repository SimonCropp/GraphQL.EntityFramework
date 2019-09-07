using ApprovalTests;
using GraphQL.EntityFramework;
using Xunit;
using Xunit.Abstractions;

public class CompressTests :
    XunitApprovalBase
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

    public CompressTests(ITestOutputHelper output) :
        base(output)
    {
    }
}