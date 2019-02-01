using Xunit;

public class PropertyCacheTests
{
    [Fact]
    public void PropertyExpression()
    {
        var target = new TargetForPropertyExpression
        {
            Member = "Value1"
        };

        var result = PropertyCache<TargetForPropertyExpression>.GetProperty("Member").Func
            .Invoke(target);
        Assert.Equal("Value1", result);
    }

    public class TargetForPropertyExpression
    {
        public string Member;
    }

    [Fact]
    public void PropertyNestedExpression()
    {
        var target = new TargetForPropertyNestedExpression
        {
            Child = new TargetChildForPropertyNestedExpression
            {
                Member = "Value1"
            }
        };

        var result = PropertyCache<TargetForPropertyNestedExpression>.GetProperty("Child.Member").Func
            .Invoke(target);
        Assert.Equal("Value1", result);
    }

    public class TargetForPropertyNestedExpression
    {
        public TargetChildForPropertyNestedExpression Child;
    }

    public class TargetChildForPropertyNestedExpression
    {
        public string Member;
    }
}