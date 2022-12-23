public class DateEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Date? Property { get; set; }
}