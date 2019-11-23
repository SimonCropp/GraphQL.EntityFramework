using System;
using Xunit;

public class WithMisNamedQueryChildEntity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid? ParentId { get; set; }
    public WithMisNamedQueryParentEntity? Parent { get; set; }
}