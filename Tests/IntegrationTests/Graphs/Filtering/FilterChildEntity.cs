using System;

public class FilterChildEntity
{
    public Guid Id { get; set; }
    public string Property { get; set; }
    public FilterParentEntity Parent { get; set; }
}