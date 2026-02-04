public class ReadOnlyEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Read-only computed property (no setter)
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public int Age { get; set; }

    // Read-only expression-bodied property
    public bool IsAdult => Age >= 18;

    // Read-only auto-property (simulates database computed column like User.DisplayName)
    public string ComputedInDb { get; } = null!;

    public Guid? ReadOnlyParentId { get; set; }
    public ReadOnlyParentEntity? ReadOnlyParent { get; set; }
}
