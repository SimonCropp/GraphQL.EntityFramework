public class FilterReferenceEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }

    // Navigation to base type (like Accommodation -> TravelRequest)
    public Guid BaseEntityId { get; set; }
    public FilterBaseEntity? BaseEntity { get; set; }
}
