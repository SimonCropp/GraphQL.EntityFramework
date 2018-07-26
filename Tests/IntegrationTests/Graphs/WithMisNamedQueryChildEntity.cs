using System;

public class WithMisNamedQueryChildEntity
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public WithMisNamedQueryParentEntity Parent { get; set; }
}