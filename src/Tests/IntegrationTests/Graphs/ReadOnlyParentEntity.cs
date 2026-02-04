public class ReadOnlyParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<ReadOnlyEntity> Children { get; set; } = [];
}
