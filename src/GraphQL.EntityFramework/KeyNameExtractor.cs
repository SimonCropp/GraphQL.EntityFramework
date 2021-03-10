using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

static class KeyNameExtractor
{
    public static IReadOnlyDictionary<Type, List<string>> GetKeyNames(this IModel model)
    {
        Dictionary<Type, List<string>> keyNames = new();
        foreach (var entityType in model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            //This can happen for views
            if (primaryKey == null)
            {
                continue;
            }

            if (entityType.IsOwned())
            {
                continue;
            }

            var names = primaryKey.Properties.Select(x => x.Name).ToList();
            keyNames.Add(entityType.ClrType, names);
        }

        return keyNames;
    }
}