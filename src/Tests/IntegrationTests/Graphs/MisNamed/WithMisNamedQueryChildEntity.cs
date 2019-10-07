using System;

public class WithMisNamedQueryChildEntity
{
    public Guid Id { get; set; } = XunitLogging.Context.NextGuid();
    public Guid? ParentId { get; set; }
    public WithMisNamedQueryParentEntity? Parent { get; set; }
}