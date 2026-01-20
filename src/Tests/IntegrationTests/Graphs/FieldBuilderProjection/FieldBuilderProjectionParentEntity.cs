public class FieldBuilderProjectionParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public List<FieldBuilderProjectionEntity> Children { get; set; } = [];
}
