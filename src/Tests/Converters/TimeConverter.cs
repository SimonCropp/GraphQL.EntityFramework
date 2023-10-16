public class TimeConverter() :
    ValueConversion.ValueConverter<Time, TimeSpan>(
        _ => _.ToTimeSpan(),
        _ => Time.FromTimeSpan(_));