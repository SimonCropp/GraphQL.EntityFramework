public class PropertyCacheTests
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

    [Fact]
    public void PropertyOnInheritedInterface()
    {
        var target = new MultiInterfaceTarget
        {
            A = "valueA",
            B = "valueB"
        };

        // Both members are declared on separate inherited interfaces. Looking up each one
        // exercises whichever interface Type.GetInterfaces() returns second, which previously
        // threw because the interface walk stopped after the first interface. Asserting both
        // makes the test independent of the (unspecified) GetInterfaces() ordering.
        var a = PropertyCache<IMultiInterface>.GetProperty("A").Func(target);
        var b = PropertyCache<IMultiInterface>.GetProperty("B").Func(target);

        Assert.Equal("valueA", a);
        Assert.Equal("valueB", b);
    }

    public interface IHasA
    {
        string? A { get; }
    }

    public interface IHasB
    {
        string? B { get; }
    }

    public interface IMultiInterface :
        IHasA,
        IHasB;

    public class MultiInterfaceTarget :
        IMultiInterface
    {
        public string? A { get; set; }
        public string? B { get; set; }
    }
}