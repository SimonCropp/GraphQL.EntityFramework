public class NullableTimeConverter() :
    ValueConversion.ValueConverter<Time?, TimeSpan?>(
        _ => _ == null ?
            null :
            new TimeSpan?(_.Value.ToTimeSpan()),
        _ => _ == null ?
            null :
            new Time?(Time.FromTimeSpan(_.Value)));