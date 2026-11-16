public class PropertyCacheTests
{
    [Test]
    public async Task Property()
    {
        var target = new TargetForProperty
        {
            Member = "Value1"
        };

        var property = PropertyCache<TargetForProperty>.GetProperty("Member");
        var result = property.Func(target);
        await Assert.That(result).IsEqualTo("Value1");
    }

    public class TargetForProperty
    {
        public string? Member;
    }

    [Test]
    public async Task PropertyNested()
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
        await Assert.That(result).IsEqualTo("Value1");
    }

    public class TargetForPropertyNested
    {
        public TargetChildForPropertyNested? Child;
    }

    public class TargetChildForPropertyNested
    {
        public string? Member;
    }

    [Test]
    public async Task PropertyOnInheritedInterface()
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

        await Assert.That(a).IsEqualTo("valueA");
        await Assert.That(b).IsEqualTo("valueB");
    }

    [Test]
    public async Task DeepPathIsRejected()
    {
        // paths come from the client, and each segment becomes a join on the queryable paths
        var path = string.Join('.', Enumerable.Repeat("Self", 11)) + ".Public";
        var exception = Assert.ThrowsExactly<Exception>(() => PropertyCache<SelfReferencing>.GetProperty(path));
        await Assert.That(exception.Message).Contains("exceeds the maximum depth");
    }

    [Test]
    public async Task PathAtMaximumDepthIsAllowed()
    {
        var path = string.Join('.', Enumerable.Repeat("Self", 9)) + ".Public";
        var property = PropertyCache<SelfReferencing>.GetProperty(path);
        await Assert.That(property.PropertyType).IsEqualTo(typeof(string));
    }

    [Test]
    public async Task CacheDoesNotGrowWithoutBound()
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

        await Assert.That(PropertyCache<SelfReferencing>.CachedCount).IsLessThanOrEqualTo(1000);
    }

    [Test]
    public async Task FuncStillWorksAfterBeingCompiledLazily()
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
        await Assert.That(property.Func(target)).IsEqualTo("Value1");
        await Assert.That(property.Func(target)).IsEqualTo("Value1");
    }

    public class SelfReferencing
    {
        public SelfReferencing? Self { get; set; }
        public string? Public { get; set; }
    }

    [Test]
    // paths come from the client, so non public members must not be resolvable
    [Arguments("secretField")]
    [Arguments("SecretProperty")]
    // resolution is case insensitive, so casing must not be a way around it
    [Arguments("SECRETFIELD")]
    [Arguments("secretproperty")]
    // nor may they be reached through a nested path
    [Arguments("Child.secretField")]
    public async Task NonPublicMembersAreNotResolved(string path)
    {
        var exception = Assert.ThrowsExactly<Exception>(() => PropertyCache<TargetWithNonPublicMembers>.GetProperty(path));
        await Assert.That(exception.Message).Contains("Failed to create a member expression");
    }

    [Test]
    public async Task PublicMembersStillResolveOnTypeWithNonPublicMembers()
    {
        var target = new TargetWithNonPublicMembers
        {
            Public = "Value1"
        };

        var property = PropertyCache<TargetWithNonPublicMembers>.GetProperty("Public");
        await Assert.That(property.Func(target)).IsEqualTo("Value1");
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
