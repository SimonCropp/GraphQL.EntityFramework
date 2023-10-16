public class NullableDateConverter() :
    ValueConversion.ValueConverter<Date?, DateTime?>(
        _ => _ == null
            ? null
            : new DateTime?(_.Value.ToDateTime(Time.MinValue)),
        _ => _ == null
            ? null
            : new Date?(Date.FromDateTime(_.Value)));