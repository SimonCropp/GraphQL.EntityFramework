using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

public class NamedIdEntity
{
    [Key]
    public Guid NamedId { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
}