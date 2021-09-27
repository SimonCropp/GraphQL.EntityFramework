using System.ComponentModel.DataAnnotations.Schema;

[Table("parent")]
public class MappingParent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<MappingChild> Children { get; set; } = new List<MappingChild>();
    public IList<string> JsonProperty { get; set; } = new List<string>();
}