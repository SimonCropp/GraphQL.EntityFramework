using Xunit;

public class NavigationLookupTests
{
    [Theory]
    [InlineData("DisplayName", "DisplayName")]  // Exact match
    [InlineData("DisplayName", "displayName")]  // camelCase query
    [InlineData("DisplayName", "displayname")]  // lowercase query
    [InlineData("DisplayName", "DISPLAYNAME")]  // uppercase query
    [InlineData("DisplayName", "DiSpLaYnAmE")]  // mixed case query
    public void Dictionary_lookup_should_match_with_case_insensitive_comparer(string propertyName, string queryName)
    {
        // Simulate how NavigationReader creates the dictionary
        var navigationList = new[] { new { Name = propertyName } };
        var dictionary = navigationList.ToDictionary(
            _ => _.Name.ToLowerInvariant(),
            StringComparer.OrdinalIgnoreCase);

        // Simulate the old FirstOrDefault approach
        var oldResult = navigationList.FirstOrDefault(n =>
            n.Name.Equals(queryName, StringComparison.OrdinalIgnoreCase));

        // Simulate the new TryGetValue approach
        var newResult = dictionary.TryGetValue(queryName, out var value) ? value : null;

        // Both should return the same result
        Assert.Equal(oldResult?.Name, newResult?.Name);
    }

    [Fact]
    public void Dictionary_with_lowercase_keys_and_case_insensitive_comparer_should_work()
    {
        // Create dictionary like NavigationReader does
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "displayname", "value1" },  // lowercase key
        };

        // Test various casings
        Assert.True(dict.TryGetValue("displayname", out var result1));
        Assert.Equal("value1", result1);

        Assert.True(dict.TryGetValue("DisplayName", out var result2));
        Assert.Equal("value1", result2);

        Assert.True(dict.TryGetValue("displayName", out var result3));
        Assert.Equal("value1", result3);

        Assert.True(dict.TryGetValue("DISPLAYNAME", out var result4));
        Assert.Equal("value1", result4);
    }
}
