using System;
using System.ComponentModel.DataAnnotations;

public class NamedIdEntity
{
    [Key]
    public Guid NamedId { get; set; } = XunitLogging.Context.NextGuid();
    public string? Property { get; set; }
}