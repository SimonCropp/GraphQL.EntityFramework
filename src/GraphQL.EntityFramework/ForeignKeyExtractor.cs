static class ForeignKeyExtractor
{
    public static IReadOnlyDictionary<Type, IReadOnlySet<string>> GetForeignKeyProperties(IModel model)
    {
        var dictionary = new Dictionary<Type, IReadOnlySet<string>>();
        foreach (var entityType in model.GetEntityTypes())
        {
            if (!dictionary.ContainsKey(entityType.ClrType))
            {
                var foreignKeys = GetForeignKeys(entityType);
                if (foreignKeys.Count > 0)
                {
                    dictionary[entityType.ClrType] = foreignKeys;
                }
            }
        }

        return dictionary;
    }

    static IReadOnlySet<string> GetForeignKeys(IEntityType entity)
    {
        var foreignKeyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var foreignKey in entity.GetForeignKeys())
        {
            foreach (var property in foreignKey.Properties)
            {
                foreignKeyNames.Add(property.Name);
            }
        }

        return foreignKeyNames;
    }
}
