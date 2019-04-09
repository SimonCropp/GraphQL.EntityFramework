using System;

public class FilterChildEntity
{
    public Guid Id { get; set; } = SimpleSequentialGuid.NewGuid();
    public string Property { get; set; }
    public FilterParentEntity Parent { get; set; }
}