[Table("parent")]
public class MappingParent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<MappingChild> Children { get; set; } = [];
    public IList<string> JsonProperty { get; set; } = [];
    public string? IgnoreByName { get; set; }
}