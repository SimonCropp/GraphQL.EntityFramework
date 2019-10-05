using Xunit;
using Xunit.Abstractions;

public class PropertyCacheTests :
    XunitApprovalBase
{
    [Fact]
    public void Property()
    {
        var target = new TargetForProperty
        {
            Member = "Value1"
        };

        var property = PropertyCache<TargetForProperty>.GetProperty("Member");
        var result = property.Func(target);
        Assert.Equal("Value1", result);
    }

    public class TargetForProperty
    {
        public string? Member;
    }

    [Fact]
    public void PropertyNested()
    {
        var target = new TargetForPropertyNested
        {
            Child = new TargetChildForPropertyNested
            {
                Member = "Value1"
            }
        };

        var property = PropertyCache<TargetForPropertyNested>.GetProperty("Child.Member");
        var result = property.Func(target);
        Assert.Equal("Value1", result);
    }

    public class TargetForPropertyNested
    {
        public TargetChildForPropertyNested? Child;
    }

    public class TargetChildForPropertyNested
    {
        public string? Member;
    }

    public PropertyCacheTests(ITestOutputHelper output) :
        base(output)
    {
    }
}