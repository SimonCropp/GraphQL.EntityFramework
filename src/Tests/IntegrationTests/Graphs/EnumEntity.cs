public class EnumEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DayOfWeek? Property { get; set; }
}