public class NonNullChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NonNullParentEntityId { get; set; }
    public NonNullParentEntity? NonNullParentEntity { get; set; }
    public string? Property { get; set; }
}