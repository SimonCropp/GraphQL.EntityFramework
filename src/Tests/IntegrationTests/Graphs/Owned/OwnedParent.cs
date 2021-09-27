public class OwnedParent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }

    public OwnedChild Child1 { get; set; } = null!;
    public OwnedChild Child2 { get; set; } = null!;
}