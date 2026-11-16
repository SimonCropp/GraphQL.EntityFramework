public class CompressTests
{
    [Test]
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

    [Test]
    public Task StringLiteralIsPreserved()
    {
        // whitespace inside a literal is significant, and stripping it silently queried for
        // different data rather than raising an error
        var query = """{users(where: {path: "name", comparison: equal, value: "Smith, John: Jr"}){id}}""";
        return Verify(Compress.Query(query))
            .Snapshot("""{users(where:{path:"name",comparison:equal,value:"Smith, John: Jr"}){id}}""");
    }

    [Test]
    public Task EscapedQuoteDoesNotEndTheLiteral()
    {
        var query = """{users(where: {value: "a \" b,  c"}){id}}""";
        return Verify(Compress.Query(query))
            .Snapshot("""{users(where:{value:"a \" b,  c"}){id}}""");
    }

    [Test]
    public Task BlockStringIsPreserved()
    {
        var query = "{users(where: {value: \"\"\"a,  b: c\"\"\"}){id}}";
        return Verify(Compress.Query(query))
            .Snapshot("{users(where:{value:\"\"\"a,  b: c\"\"\"}){id}}");
    }

    [Test]
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

    [Test]
    public Task NamesStaySeparated() =>
        Verify(Compress.Query("query Named { users { id name } }"))
            .Snapshot("query Named{users{id name}}");
}