using System.Globalization;

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
        var result = TypeConverter.ConvertStringToType("10:01", typeof(Time));
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

    // Where values are query text, so they parse the same under every server culture, and a
    // single comparison and an `in` list read the same literal the same way
    public static IEnumerable<object?[]> CultureCases()
    {
        string[] cultures = ["de-DE", "fr-FR", "en-US", "ar-SA"];
        (Type type, string value, object expected)[] cases =
        [
            (typeof(decimal), "1.5", 1.5m),
            (typeof(decimal), "1,234.5", 1234.5m),
            (typeof(double), "1.5", 1.5d),
            (typeof(float), "1.5", 1.5f),
            (typeof(int), "-12", -12),
            (typeof(long), "12", 12L),
            (typeof(short), "12", (short)12),
            (typeof(byte), "12", (byte)12),
            (typeof(DateTime), "2020-10-01T10:11:12Z", new DateTime(2020, 10, 1, 10, 11, 12, DateTimeKind.Utc)),
            (typeof(DateTimeOffset), "2020-10-01T10:11:12+02:00", new DateTimeOffset(2020, 10, 1, 10, 11, 12, TimeSpan.FromHours(2))),
            (typeof(Date), "2020-10-01", new Date(2020, 10, 1)),
            (typeof(Time), "10:11:12", new Time(10, 11, 12)),
        ];
        foreach (var culture in cultures)
        {
            foreach (var (type, value, expected) in cases)
            {
                yield return [culture, type, value, expected];
            }
        }
    }

    [Theory]
    [MemberData(nameof(CultureCases))]
    public void Parses_invariant_of_culture(string culture, Type type, string value, object expected)
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new(culture);
        try
        {
            var single = TypeConverter.ConvertStringToType(value, type);
            Assert.Equal(expected, single);

            var member = typeof(NullableHolder).GetProperty(type.Name)!;
            var list = TypeConverter.ConvertStringsToList([value, null], member);
            Assert.Equal(2, list.Count);
            Assert.Equal(expected, list[0]);
            Assert.Null(list[1]);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void List_of_non_nullable_member_type()
    {
        var list = TypeConverter.ConvertStringsToList(["1", "2"], typeof(Holder).GetProperty(nameof(Holder.Value))!);
        Assert.IsType<List<int>>(list);
        Assert.Equal([1, 2], list.Cast<int>());
    }

    [Fact]
    public void List_of_nullable_member_type()
    {
        var list = TypeConverter.ConvertStringsToList(["1", null], typeof(Holder).GetProperty(nameof(Holder.Nullable))!);
        Assert.IsType<List<int?>>(list);
        Assert.Equal([1, null], list.Cast<int?>());
    }

    class Holder
    {
        public int Value { get; set; }
        public int? Nullable { get; set; }
    }

    // One nullable property per type, named after the type, for the list conversion
    class NullableHolder
    {
        public decimal? Decimal { get; set; }
        public double? Double { get; set; }
        public float? Single { get; set; }
        public int? Int32 { get; set; }
        public long? Int64 { get; set; }
        public short? Int16 { get; set; }
        public byte? Byte { get; set; }
        public DateTime? DateTime { get; set; }
        public DateTimeOffset? DateTimeOffset { get; set; }
        public Date? DateOnly { get; set; }
        public Time? TimeOnly { get; set; }
    }
}
