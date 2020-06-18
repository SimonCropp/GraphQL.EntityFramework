using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("child")]
public class MappingChild
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid ParentId { get; set; }
    public MappingParent Parent { get; set; } = null!;
}