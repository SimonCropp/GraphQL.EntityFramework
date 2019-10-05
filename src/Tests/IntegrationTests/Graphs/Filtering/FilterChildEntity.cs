using System;

public class FilterChildEntity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public string? Property { get; set; }
    public FilterParentEntity? Parent { get; set; }
}