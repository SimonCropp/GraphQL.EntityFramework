public class TypeConverterTests
{
    [Theory]
    [InlineData(typeof(int), "12", 12)]
    [InlineData(typeof(int?), null, null)]
    public void ConvertStringToType(Type type, string? value, object? expected)
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
    public void ConvertStringToDate()
    {
        var date = new Date(2020,10,1);
        var result = TypeConverter.ConvertStringToType(date.ToString("yyyy-MM-dd"), typeof(Date));
        Assert.Equal(date, result);
    }

    [Fact]
    public void ConvertStringToTime()
    {
        var time = new Time(10,1);
        var result = TypeConverter.ConvertStringToType(time.ToString(), typeof(Time));
        Assert.Equal(time, result);
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
}