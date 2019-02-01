using Xunit;

public class PropertyCacheTests
{
    [Fact]
    public void Property()
    {
        var target = new TargetForProperty
        {
            Member = "Value1"
        };

        var result = PropertyCache<TargetForProperty>.GetProperty("Member").Func
            .Invoke(target);
        Assert.Equal("Value1", result);
    }

    public class TargetForProperty
    {
        public string Member;
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

        var result = PropertyCache<TargetForPropertyNested>.GetProperty("Child.Member").Func
            .Invoke(target);
        Assert.Equal("Value1", result);
    }

    public class TargetForPropertyNested
    {
        public TargetChildForPropertyNested Child;
    }

    public class TargetChildForPropertyNested
    {
        public string Member;
    }
}