public class CompressTests
{
    [Fact]
    public Task Simple()
    {
        var query =
            """
            query ($id: String!)
            {
              companies(ids:[$id])
              {
                id
              }
            }
            """;
        return Verify(Compress.Query(query));
    }
}