public class DateConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Date, DateTime>
{
    public DateConverter() : base(
        d => d.ToDateTime(Time.MinValue),
        d => Date.FromDateTime(d))
    {
    }
}