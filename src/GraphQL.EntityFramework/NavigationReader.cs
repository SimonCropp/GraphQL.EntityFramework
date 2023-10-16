static class NavigationReader
{
    public static IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> GetNavigationProperties(IModel model)
    {
        var dictionary = new Dictionary<Type, IReadOnlyList<Navigation>>();
        foreach (var property in model.GetEntityTypes())
        {
            if (!dictionary.ContainsKey(property.ClrType))
            {
                dictionary[property.ClrType] = GetNavigations(property);
            }
        }

        return dictionary;
    }

    static IReadOnlyList<Navigation> GetNavigations(IEntityType entity)
    {
        var navigations = [..entity.GetNavigations(), ..entity.GetSkipNavigations()];
        var idProperties = entity.GetProperties()
            .Where(_ => _.Name.EndsWith("Id"))
            .ToDictionary(
                _ => _.Name,
                _.PropertyInfo!.IsNullable());
        return navigations
            .Select(
                _ =>
                {
                    var (itemType, isCollection) = GetNavigationType(_);
                    var isNullable = _.PropertyInfo!.IsNullable();
                    if (isNullable)
                    {
                        if (idProperties.TryGetValue(_.Name + "Id", out var isIdNullable))
                        {
                            isNullable = isIdNullable;
                        }
                    }

                    return new Navigation(_.Name, itemType, isNullable, isCollection);
                })
            .ToList();
    }

    static (Type itemType, bool isCollection) GetNavigationType(INavigationBase navigation)
    {
        var navigationType = navigation.ClrType;
        if (navigationType.TryGetCollectionType(out var collectionGenericType))
        {
            return (collectionGenericType, true);
        }

        return (navigationType, false);
    }
}