public class FilterDerivedEntity : FilterBaseEntity
{
    public string? DerivedProperty { get; set; }
    public IList<FilterReferenceEntity> References { get; set; } = [];
}
