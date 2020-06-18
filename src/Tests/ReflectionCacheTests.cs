using Xunit;

public class ReflectionCacheTests
{
    [Fact]
    public void TryGetCollectionType_String()
    {
        Assert.False(typeof(string).TryGetCollectionType(out var itemType));
        Assert.Null(itemType);
    }
}