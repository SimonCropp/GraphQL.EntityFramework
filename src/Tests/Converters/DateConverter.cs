public class DateConverter() :
    ValueConversion.ValueConverter<Date, DateTime>(
        d => d.ToDateTime(Time.MinValue),
        d => Date.FromDateTime(d));