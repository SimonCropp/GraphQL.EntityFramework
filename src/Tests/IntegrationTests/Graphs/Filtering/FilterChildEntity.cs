using System;
using Xunit;

public class FilterChildEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
    public FilterParentEntity? Parent { get; set; }
}