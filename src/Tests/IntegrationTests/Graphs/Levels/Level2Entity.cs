using System;
using Xunit;

public class Level2Entity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public Guid? Level3EntityId { get; set; }
    public Level3Entity? Level3Entity { get; set; }
}