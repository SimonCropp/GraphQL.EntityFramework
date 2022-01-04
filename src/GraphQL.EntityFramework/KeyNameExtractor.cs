using Microsoft.EntityFrameworkCore.Metadata;

static class KeyNameExtractor
{
    public static IReadOnlyDictionary<Type, List<string>> GetKeyNames(this IModel model)
    {
        var keyNames = new Dictionary<Type, List<string>>();
        foreach (var entityType in model.GetEntityTypes())
        {
            var primaryKey = entityType.FindPrimaryKey();
            //This can happen for views
            if (primaryKey is null)
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