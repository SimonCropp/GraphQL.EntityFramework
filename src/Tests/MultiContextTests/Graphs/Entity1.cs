using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

[Table("entity1")]
public class Entity1
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
}