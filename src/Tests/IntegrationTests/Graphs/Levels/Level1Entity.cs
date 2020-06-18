using System;

public class Level1Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? Level2EntityId { get; set; }
    public Level2Entity? Level2Entity { get; set; }
}