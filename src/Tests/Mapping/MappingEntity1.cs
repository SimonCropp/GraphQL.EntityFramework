using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

[Table("entity1")]
public class MappingEntity1
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
}