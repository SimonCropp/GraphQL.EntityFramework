using System;
using System.Collections.Generic;
using Xunit;

public class FilterParentEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public string? Property { get; set; }
    public IList<FilterChildEntity> Children { get; set; } = new List<FilterChildEntity>();
}