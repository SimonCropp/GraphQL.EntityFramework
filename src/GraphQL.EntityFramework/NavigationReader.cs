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
            .ToDictionary(x => x.ClrType, GetNavigations);
    }

    static IReadOnlyList<Navigation> GetNavigations(IEntityType entity)
    {
        var navigations = entity.GetNavigations();
        return navigations
            .Select(x =>
            {
                var (itemType, isCollection) = GetNavigationType(x);
                return new Navigation(x.Name, itemType, x.PropertyInfo.IsNullable(),isCollection);
            })
            .ToList();
    }

    static (Type itemType, bool isCollection) GetNavigationType(INavigation navigation)
    {
        var navigationType = navigation.ClrType;
        var collectionType = navigationType.GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType &&
                                  x.GetGenericTypeDefinition() == typeof(ICollection<>));
        if (collectionType == null)
        {
            return (navigationType, false);
        }

        return (collectionType.GetGenericArguments().Single(), true);
    }
}