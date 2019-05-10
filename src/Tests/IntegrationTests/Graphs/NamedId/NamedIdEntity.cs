using System;
using System.ComponentModel.DataAnnotations;

public class NamedIdEntity
{
    [Key]
    public Guid NamedId { get; set; } = SimpleSequentialGuid.NewGuid();
    public string Property { get; set; }
}