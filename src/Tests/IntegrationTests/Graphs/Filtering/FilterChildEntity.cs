public class FilterChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? NullableAge { get; set; }
    public bool? NullableIsActive { get; set; }
    public DateTime? NullableCreatedAt { get; set; }
    public FilterParentEntity? Parent { get; set; }
}