public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public string? Status { get; set; } = "Draft";
    public IList<DerivedChildEntity> ChildrenFromBase { get; set; } = [];
}