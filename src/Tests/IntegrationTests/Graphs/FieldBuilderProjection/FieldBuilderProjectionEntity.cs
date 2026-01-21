public enum EntityStatus
{
    Active,
    Inactive,
    Pending
}

public class FieldBuilderProjectionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Salary { get; set; }
    public double Score { get; set; }
    public long ViewCount { get; set; }
    public EntityStatus Status { get; set; }
    public FieldBuilderProjectionParentEntity? Parent { get; set; }
}
