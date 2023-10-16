public class DateConverter() :
    ValueConversion.ValueConverter<Date, DateTime>(
        _ => _.ToDateTime(Time.MinValue),
        _ => Date.FromDateTime(_));