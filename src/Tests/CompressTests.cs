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
        return Verify(Compress.Query(query))
            .Snapshot("query($id:String!){companies(ids:[$id]){id}}");
    }

    [Fact]
    public Task StringLiteralIsPreserved()
    {
        // whitespace inside a literal is significant, and stripping it silently queried for
        // different data rather than raising an error
        var query = """{users(where: {path: "name", comparison: equal, value: "Smith, John: Jr"}){id}}""";
        return Verify(Compress.Query(query))
            .Snapshot("""{users(where:{path:"name",comparison:equal,value:"Smith, John: Jr"}){id}}""");
    }

    [Fact]
    public Task EscapedQuoteDoesNotEndTheLiteral()
    {
        var query = """{users(where: {value: "a \" b,  c"}){id}}""";
        return Verify(Compress.Query(query))
            .Snapshot("""{users(where:{value:"a \" b,  c"}){id}}""");
    }

    [Fact]
    public Task BlockStringIsPreserved()
    {
        var query = "{users(where: {value: \"\"\"a,  b: c\"\"\"}){id}}";
        return Verify(Compress.Query(query))
            .Snapshot("{users(where:{value:\"\"\"a,  b: c\"\"\"}){id}}");
    }

    [Fact]
    public Task CommentIsRemovedWithoutSwallowingTheQuery()
    {
        // collapsing the newline into a space would pull the rest of the query into the comment
        var query = """
                    {
                      # a comment
                      users
                      {
                        id
                      }
                    }
                    """;
        return Verify(Compress.Query(query))
            .Snapshot("{users{id}}");
    }

    [Fact]
    public Task NamesStaySeparated() =>
        Verify(Compress.Query("query Named { users { id name } }"))
            .Snapshot("query Named{users{id name}}");
}