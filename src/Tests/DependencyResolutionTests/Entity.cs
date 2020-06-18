using System;

public class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
}