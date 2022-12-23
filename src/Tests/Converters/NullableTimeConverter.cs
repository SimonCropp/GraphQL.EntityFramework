public class NullableTimeConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Time?, TimeSpan?>
{
    public NullableTimeConverter() : base(
        t => t == null ? null : new TimeSpan?(t.Value.ToTimeSpan()),
        t => t == null ? null : new Time?(Time.FromTimeSpan(t.Value)))
    {
    }
}