public class TimeEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Time? Property { get; set; }
}