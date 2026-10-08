/// <summary>
/// Sets a navigation on an entity the projection has already created. A type with derived types is
/// projected as a chain of type tests, each creating its own type, and a navigation that loads a
/// collection is bound once to the result of that chain rather than inside every branch of it.
/// There is no expression for assigning a property of an existing instance that EF accepts in a
/// projection, so it is a call EF evaluates on the client, as it does the member inits.
/// </summary>
static class HoistedNavigation
{
    static MethodInfo setMethod = typeof(HoistedNavigation)
        .GetMethod(nameof(Set), BindingFlags.Static | BindingFlags.NonPublic)!;

    static ConcurrentDictionary<PropertyInfo, MethodInfo> setMethods = new();

    public static Expression Assign(Expression entity, Type entityType, PropertyInfo property, Expression value)
    {
        var method = setMethods.GetOrAdd(
            property,
            _ => setMethod.MakeGenericMethod(_.ReflectedType!, _.PropertyType));

        if (entity.Type != entityType)
        {
            entity = Expression.Convert(entity, entityType);
        }

        if (value.Type != property.PropertyType)
        {
            value = Expression.Convert(value, property.PropertyType);
        }

        // The name is a constant, so the expression, and with it EF's compiled query cache entry,
        // depends only on which navigations were requested
        return Expression.Call(null, method, entity, value, Expression.Constant(property.Name));
    }

    static TEntity? Set<TEntity, TValue>(TEntity? entity, TValue value, string name)
        where TEntity : class
    {
        if (entity is not null)
        {
            Setter<TEntity, TValue>.Get(name)(entity, value);
        }

        return entity;
    }

    static class Setter<TEntity, TValue>
        where TEntity : class
    {
        static ConcurrentDictionary<string, Action<TEntity, TValue>> setters = new();

        public static Action<TEntity, TValue> Get(string name) =>
            setters.GetOrAdd(
                name,
                _ =>
                {
                    var property = typeof(TEntity).GetProperty(_, BindingFlags.Instance | BindingFlags.Public)!;
                    var entity = Expression.Parameter(typeof(TEntity));
                    var value = Expression.Parameter(typeof(TValue));
                    var assign = Expression.Assign(Expression.Property(entity, property), value);
                    return Expression.Lambda<Action<TEntity, TValue>>(assign, entity, value).Compile();
                });
    }
}
