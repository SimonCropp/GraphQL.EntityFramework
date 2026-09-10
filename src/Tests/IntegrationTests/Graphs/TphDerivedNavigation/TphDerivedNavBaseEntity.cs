public abstract class TphDerivedNavBaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid? OwnerId { get; set; }
}
