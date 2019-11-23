using System;
using Xunit;

public class Level1Entity
{
    public Guid Id { get; set; } = XunitContext.Context.NextGuid();
    public int? Level2EntityId { get; set; }
    public Level2Entity? Level2Entity { get; set; }
}