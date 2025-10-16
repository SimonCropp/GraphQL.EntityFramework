public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<DerivedChildEntity> ChildrenFromBase { get; set; } = [];
}