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
            .Select(x => new Navigation(x.Name, GetNavigationType(x)))
            .ToList();
    }

    static Type GetNavigationType(INavigation navigation)
    {
        var navigationType = navigation.ClrType;
        var collectionType = navigationType.GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType &&
                                  x.GetGenericTypeDefinition() == typeof(ICollection<>));
        if (collectionType == null)
        {
            return navigationType;
        }

        return collectionType.GetGenericArguments().Single();
    }
}