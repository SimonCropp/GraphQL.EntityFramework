using System;
using Xunit;
using Xunit.Abstractions;

public class TypeConverterTests :
    XunitApprovalBase
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

    [Fact]
    public void ConvertStringToEnum()
    {
        var day = DayOfWeek.Thursday;
        var value = day.ToString();
        var result = TypeConverter.ConvertStringToType(value, typeof(DayOfWeek));
        Assert.Equal(day, result);
    }

    [Fact]
    public void ConvertUppercaseStringToEnum()
    {
        var day = DayOfWeek.Thursday;
        var value = day.ToString().ToUpperInvariant();
        var result = TypeConverter.ConvertStringToType(value, typeof(DayOfWeek));
        Assert.Equal(day, result);
    }

    [Theory]
    [InlineData(new[] { "876f5560-d0f8-4930-8089-d0d58a989928", "D5DE2533-ABCE-4870-A839-1C5E8781905F" }, typeof(Guid))]
    [InlineData(new[] { "876f5560-d0f8-4930-8089-d0d58a989928", "D5DE2533-ABCE-4870-A839-1C5E8781905F" }, typeof(Guid?))]
    [InlineData(new[] { "false", "true", "True", "False", "0", "1" }, typeof(bool), new[] { "false", "true", "true", "false", "false", "true" })]
    [InlineData(new[] { "false", "true", "True", "False", "0", "1" }, typeof(bool?), new[] { "false", "true", "true", "false", "false", "true" })]
    [InlineData(new[] { "0", "1", "-1", "12342" }, typeof(int))]
    [InlineData(new[] { "0", "1", "-1", "12342" }, typeof(int?))]
    [InlineData(new[] { "0", "1", "-1", "12342" }, typeof(short))]
    [InlineData(new[] { "0", "1", "-1", "12342" }, typeof(short?))]
    [InlineData(new[] { "0", "1", "-1", "12342" }, typeof(long))]
    [InlineData(new[] { "0", "1", "-1", "12342" }, typeof(long?))]
    [InlineData(new[] { "0", "1", "12342" }, typeof(uint))]
    [InlineData(new[] { "0", "1", "12342" }, typeof(uint?))]
    [InlineData(new[] { "0", "1", "12342" }, typeof(ushort))]
    [InlineData(new[] { "0", "1", "12342" }, typeof(ushort?))]
    [InlineData(new[] { "0", "1", "12342" }, typeof(ulong))]
    [InlineData(new[] { "0", "1", "12342" }, typeof(ulong?))]
    [InlineData(new[] { "2019-06-14 0:00", "1970-01-01 14:33", "2233-03-22 0:00" }, typeof(DateTime))]
    [InlineData(new[] { "2019-06-14 0:00", "1970-01-01 14:33", "2233-03-22 0:00" }, typeof(DateTime?))]
    [InlineData(new[] { "2019-06-14 0:00", "1970-01-01 14:33", "2233-03-22 0:00" }, typeof(DateTimeOffset))]
    [InlineData(new[] { "2019-06-14 0:00", "1970-01-01 14:33", "2233-03-22 0:00" }, typeof(DateTimeOffset?))]
    public void ConvertStringsToList(string[] values, Type type, string[]? expectedValues = null)
    {
        var results = TypeConverter.ConvertStringsToList(values, type);
        var listContains = ReflectionCache.GetListContains(type)!;
        Assert.Equal(values.Length, results.Count);
        for (var i = 0; i < values.Length; i++)
        {
            string actual;
            var result = results[i];
            var expected = expectedValues?[i] ?? values[i];
            if (result is DateTime || result is DateTimeOffset)
            {
                actual = $"{result:yyyy-MM-dd H:mm}";
            }
            else
            {
                actual = Convert.ToString(result)!;
            }

            Assert.Equal(expected, actual, ignoreCase: true);

            var convertType = type.IsGenericType ? type.GenericTypeArguments[0] : type;
            var contains = (bool)listContains.Invoke(results, new[] { Convert.ChangeType(results[0], convertType) })!;
            Assert.True(contains);
        }
    }

    public TypeConverterTests(ITestOutputHelper output) :
        base(output)
    {
    }
}