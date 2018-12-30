using System;
using Xunit;

public class TypeConverterTests
{
    [Theory]
    [InlineData(typeof(int), "12", 12)]
    [InlineData(typeof(int?), null, null)]
    public void ConvertStringToType(Type type, string value, object expected)
    {
        var result = TypeConverter.ConvertStringToType(value, type);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertStringToGuid()
    {
        var guid = Guid.NewGuid();
        var value = guid.ToString();
        var result = TypeConverter.ConvertStringToType(value, typeof(Guid));
        Assert.Equal(guid, result);
    }

    [Fact]
    public void ConvertStringToDatetime()
    {
        var dateTime = DateTime.UtcNow.Date;
        var value = dateTime.ToString("o");
        var result = TypeConverter.ConvertStringToType(value, typeof(DateTime));
        Assert.Equal(dateTime, result);
    }
}