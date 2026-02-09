public abstract class DiscriminatorBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DiscriminatorType EntityType { get; set; }
    public string? Property { get; set; }
}
