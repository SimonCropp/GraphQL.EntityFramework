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

        // A query for this type also returns its derived types, each projected as itself, so the
        // foreign keys they declare are loaded too. A name is only bound on the types that have it.
        foreach (var derived in entity.GetDerivedTypes())
        {
            foreach (var foreignKey in derived.GetDeclaredForeignKeys())
            {
                foreach (var property in foreignKey.Properties)
                {
                    foreignKeyNames.Add(property.Name);
                }
            }
        }

        // Include TPH discriminator property so projected entities maintain correct type identity.
        // Without this, projected entities get the default discriminator value (e.g. enum value 0)
        // instead of the actual value, causing downstream code that switches on the discriminator to fail.
        var discriminator = entity.FindDiscriminatorProperty();
        if (discriminator != null)
        {
            foreignKeyNames.Add(discriminator.Name);
        }

        return foreignKeyNames;
    }
}
