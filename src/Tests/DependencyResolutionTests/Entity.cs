using System;

public class Entity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string? Property { get; set; }
}