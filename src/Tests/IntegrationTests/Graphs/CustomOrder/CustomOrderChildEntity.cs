public class CustomOrderChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentId { get; set; }
    public CustomOrderParentEntity? Parent { get; set; }
}