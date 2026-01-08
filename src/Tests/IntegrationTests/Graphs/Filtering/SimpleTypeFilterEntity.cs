public class SimpleTypeFilterEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public int IntValue { get; set; }
    public int? NullableIntValue { get; set; }
    public bool BoolValue { get; set; }
    public bool? NullableBoolValue { get; set; }
    public DateTime DateTimeValue { get; set; } = DateTime.UtcNow;
    public DateTime? NullableDateTimeValue { get; set; }
    public Guid GuidValue { get; set; } = Guid.NewGuid();
    public Guid? NullableGuidValue { get; set; }
}
