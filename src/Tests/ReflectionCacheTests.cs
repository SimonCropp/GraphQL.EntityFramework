using VerifyXunit;
using Xunit;
using Xunit.Abstractions;

public class ReflectionCacheTests :
    VerifyBase
{
    [Fact]
    public void TryGetCollectionType_String()
    {
        Assert.False(typeof(string).TryGetCollectionType(out var itemType));
        Assert.Null(itemType);
    }

    public ReflectionCacheTests(ITestOutputHelper output) :
        base(output)
    {
    }
}