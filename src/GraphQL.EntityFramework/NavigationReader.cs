using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

static class NavigationReader
{
    public static IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> GetNavigationProperties(IModel model)
    {
        return model
            .GetEntityTypes()
            .Where(x => !x.IsOwned())
            .ToDictionary(x => x.ClrType, GetNavigations);
    }

    static IReadOnlyList<Navigation> GetNavigations(IEntityType entity)
    {
        var navigations = entity.GetNavigations()
            .Cast<INavigationBase>().Concat(entity.GetSkipNavigations());
        return navigations
            .Select(x =>
            {
                var (itemType, isCollection) = GetNavigationType(x);
                return new Navigation(x.Name, itemType, x.PropertyInfo.IsNullable(), isCollection);
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