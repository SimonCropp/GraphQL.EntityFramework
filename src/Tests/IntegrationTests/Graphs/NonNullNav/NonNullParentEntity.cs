public class NonNullParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int NonNullChildEntityId { get; set; }
    public NonNullChildEntity? NonNullChildEntity { get; set; }
    public string? Property { get; set; }
}