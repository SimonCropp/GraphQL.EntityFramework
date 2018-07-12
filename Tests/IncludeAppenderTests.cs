using System.Collections.Generic;
using EfCoreGraphQL;
using GraphQL.Language.AST;
using ObjectApproval;
using Xunit;

public class IncludeAppenderTests
{
    class MockField : Field
    {
        public MockField()
        {
            SelectionSet = new SelectionSet();
        }
    }

    [Fact]
    public void Run()
    {
        var list = new List<MockField>();
        var a = new MockField
        {
            Name = "a"
        };
        var b = new MockField
        {
            Name = "b"
        };
        a.SelectionSet.Add(b);
        var c = new MockField
        {
            Name = "c"
        };
        b.SelectionSet.Add(c);
        list.Add(a);

        ObjectApprover.VerifyWithJson(IncludeAppender.GetPaths(list));
    }
}