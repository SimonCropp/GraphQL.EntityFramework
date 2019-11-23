using System.Threading.Tasks;
using GraphQL.EntityFramework;
using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class CompressTests :
    VerifyBase
{
    [Fact]
    public Task Simple()
    {
        var query = @"
query ($id: String!)
{
  companies(ids:[$id])
  {
    id
  }
}";
        return Verify(Compress.Query(query));
    }

    public CompressTests(ITestOutputHelper output) :
        base(output)
    {
    }
}