using System.Collections.Generic;

public class DerivedWithNavigationEntity : InheritedEntity
{
    public IList<DerivedChildEntity> Children { get; set; } = new List<DerivedChildEntity>();
}