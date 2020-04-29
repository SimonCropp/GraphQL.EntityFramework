using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

[Table("child")]
public class MappingChild
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
    public Guid ParentId { get; set; }
    public MappingParent Parent { get; set; } = null!;
}