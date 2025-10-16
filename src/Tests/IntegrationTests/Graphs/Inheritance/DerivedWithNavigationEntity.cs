public class DerivedWithNavigationEntity : BaseEntity
{
    public IList<DerivedChildEntity> Children { get; set; } = [];
}