using System;
using System.ComponentModel.DataAnnotations.Schema;

[Table("entity2")]
public class Entity2
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string? Property { get; set; }
}