static class NavigationReader
{
    public static IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> GetNavigationProperties(IModel model)
    {
        var dictionary = new Dictionary<Type, IReadOnlyDictionary<string, Navigation>>();
        foreach (var property in model.GetEntityTypes())
        {
            if (!dictionary.ContainsKey(property.ClrType))
            {
                dictionary[property.ClrType] = GetNavigations(property);
            }
        }

        return dictionary;
    }

    static IReadOnlyDictionary<string, Navigation> GetNavigations(IEntityType entity)
    {
        var navigations = entity.GetNavigations()
            .Cast<INavigationBase>().Concat(entity.GetSkipNavigations());
        return navigations
            .Select(
                _ =>
                {
                    var (itemType, isCollection) = GetNavigationType(_);
                    return new Navigation(_.Name, itemType, _.PropertyInfo!.IsNullable(), isCollection);
                })
            .ToDictionary(_ => _.Name.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);
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