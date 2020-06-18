using System;

public class WithMisNamedQueryChildEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ParentId { get; set; }
    public WithMisNamedQueryParentEntity? Parent { get; set; }
}