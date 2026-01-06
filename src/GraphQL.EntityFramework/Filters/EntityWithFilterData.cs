namespace GraphQL.EntityFramework;

public class EntityWithFilterData<TEntity>
    where TEntity : class
{
    public TEntity Entity { get; set; } = null!;
    public Dictionary<Type, object> FilterData { get; set; } = new();
}
