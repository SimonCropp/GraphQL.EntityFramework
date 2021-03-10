using Xunit;

public class PropertyCacheTests
{
    [Fact]
    public void Property()
    {
        TargetForProperty target = new()
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
        TargetForPropertyNested target = new()
        {
            Child = new()
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
}