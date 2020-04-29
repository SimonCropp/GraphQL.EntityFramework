using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

[Table("parent")]
public class MappingParent
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
}