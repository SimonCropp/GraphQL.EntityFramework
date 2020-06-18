using System;
using System.Collections.Generic;

public class FilterParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<FilterChildEntity> Children { get; set; } = new List<FilterChildEntity>();
}