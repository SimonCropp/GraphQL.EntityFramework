public class CustomOrderChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid? ParentId { get; set; }
    public CustomOrderParentEntity? Parent { get; set; }
}