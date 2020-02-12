using System;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

[Table("entity2")]
public class Entity2
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
}