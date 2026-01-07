public class FilterChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public FilterParentEntity? Parent { get; set; }
}