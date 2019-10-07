using System;

public class Level3Entity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string? Property { get; set; }
}