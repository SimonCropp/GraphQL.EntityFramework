public class TimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Time, TimeSpan>
{
    public TimeConverter() : base(
        t => t.ToTimeSpan(),
        t => Time.FromTimeSpan(t))
    {
    }
}