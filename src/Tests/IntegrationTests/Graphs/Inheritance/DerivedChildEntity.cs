public class DerivedChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid? ParentId { get; set; }
    public BaseEntity? Parent { get; set; }
    public Guid? TypedParentId { get; set; }
    public DerivedWithNavigationEntity? TypedParent { get; set; }
}