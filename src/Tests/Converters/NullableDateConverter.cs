public class NullableDateConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Date?, DateTime?>
{
    public NullableDateConverter() : base(
        d => d == null
            ? null
            : new DateTime?(d.Value.ToDateTime(Time.MinValue)),
        d => d == null
            ? null
            : new Date?(Date.FromDateTime(d.Value)))
    {
    }
}