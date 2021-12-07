using GraphQL.EntityFramework;

[UsesVerify]
public class CompressTests
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
        return Verifier.Verify(Compress.Query(query));
    }
}