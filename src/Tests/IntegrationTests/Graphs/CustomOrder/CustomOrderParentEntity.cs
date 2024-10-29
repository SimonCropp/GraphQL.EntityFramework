public class CustomOrderParentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public IList<CustomOrderChildEntity> Children { get; set; } = [];
}