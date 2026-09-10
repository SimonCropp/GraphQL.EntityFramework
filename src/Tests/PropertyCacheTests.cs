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

    [Fact]
    public void DeepPathIsRejected()
    {
        // paths come from the client, and each segment becomes a join on the queryable paths
        var path = string.Join('.', Enumerable.Repeat("Self", 11)) + ".Public";
        var exception = Assert.Throws<Exception>(() => PropertyCache<SelfReferencing>.GetProperty(path));
        Assert.Contains("exceeds the maximum depth", exception.Message);
    }

    [Fact]
    public void PathAtMaximumDepthIsAllowed()
    {
        var path = string.Join('.', Enumerable.Repeat("Self", 9)) + ".Public";
        var property = PropertyCache<SelfReferencing>.GetProperty(path);
        Assert.Equal(typeof(string), property.PropertyType);
    }

    [Fact]
    public void CacheDoesNotGrowWithoutBound()
    {
        // a self referencing navigation can mint unlimited distinct valid paths
        for (var i = 1; i <= 3000; i++)
        {
            var depth = i % 9 + 1;
            var path = string.Join('.', Enumerable.Repeat("Self", depth)) + ".Public";
            PropertyCache<SelfReferencing>.GetProperty(path);

            // vary the casing to mint distinct keys for the same member
            PropertyCache<SelfReferencing>.GetProperty(path.Replace("Self", i % 2 == 0 ? "self" : "SELF"));
        }

        Assert.True(
            PropertyCache<SelfReferencing>.CachedCount <= 1000,
            $"cache grew to {PropertyCache<SelfReferencing>.CachedCount}");
    }

    [Fact]
    public void FuncStillWorksAfterBeingCompiledLazily()
    {
        var target = new SelfReferencing
        {
            Self = new()
            {
                Public = "Value1"
            }
        };

        var property = PropertyCache<SelfReferencing>.GetProperty("Self.Public");

        // invoked twice to cover the memoised path as well as the first compile
        Assert.Equal("Value1", property.Func(target));
        Assert.Equal("Value1", property.Func(target));
    }

    public class SelfReferencing
    {
        public SelfReferencing? Self { get; set; }
        public string? Public { get; set; }
    }

    [Theory]
    // paths come from the client, so non public members must not be resolvable
    [InlineData("secretField")]
    [InlineData("SecretProperty")]
    // resolution is case insensitive, so casing must not be a way around it
    [InlineData("SECRETFIELD")]
    [InlineData("secretproperty")]
    // nor may they be reached through a nested path
    [InlineData("Child.secretField")]
    public void NonPublicMembersAreNotResolved(string path)
    {
        var exception = Assert.Throws<Exception>(() => PropertyCache<TargetWithNonPublicMembers>.GetProperty(path));
        Assert.Contains("Failed to create a member expression", exception.Message);
    }

    [Fact]
    public void PublicMembersStillResolveOnTypeWithNonPublicMembers()
    {
        var target = new TargetWithNonPublicMembers
        {
            Public = "Value1"
        };

        var property = PropertyCache<TargetWithNonPublicMembers>.GetProperty("Public");
        Assert.Equal("Value1", property.Func(target));
    }

    public class TargetWithNonPublicMembers
    {
        public string? Public { get; set; }
        public TargetWithNonPublicMembers? Child { get; set; }

        // ReSharper disable once UnusedMember.Local
        string? secretField = "hunter2";

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        string? SecretProperty { get; set; } = "top-secret";

        public string ReadNonPublic() => secretField + SecretProperty;
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
