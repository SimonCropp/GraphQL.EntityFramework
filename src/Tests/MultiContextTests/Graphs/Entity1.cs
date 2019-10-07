using System;
using System.ComponentModel.DataAnnotations.Schema;


[Table("entity1")]
public class Entity1
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string? Property { get; set; }
}