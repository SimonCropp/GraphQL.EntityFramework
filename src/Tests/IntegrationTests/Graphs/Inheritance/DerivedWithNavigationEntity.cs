public class DerivedWithNavigationEntity : InheritedEntity
{
    public IList<DerivedChildEntity> Children { get; set; } = [];
}