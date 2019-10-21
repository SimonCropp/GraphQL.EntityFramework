using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

static class KeyNameExtractor
{
    public static IReadOnlyDictionary<Type, List<string>> GetKeyNames(this IModel model)
    {
        var keyNames1 = new Dictionary<Type, List<string>>();
        foreach (var entityType in model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            //This can happen for views
            if (primaryKey == null)
            {
                continue;
            }

            var names = primaryKey.Properties.Select(x => x.Name).ToList();
            keyNames1.Add(entityType.ClrType, names);
        }

        return keyNames1;
    }
}