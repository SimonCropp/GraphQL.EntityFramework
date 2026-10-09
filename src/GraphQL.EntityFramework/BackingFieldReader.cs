static class BackingFieldReader
{
    /// <summary>
    /// For each entity type, the mapped properties that have no setter, with the field EF writes
    /// each of them through. A get only auto property, typically a database computed column, is
    /// one. A member init cannot bind a property with no setter, but it can bind the field, so the
    /// select projection uses these to load such a property rather than giving up on the select.
    /// An expression bodied property has no field and is not mapped, so is not among them.
    /// </summary>
    public static IReadOnlyDictionary<Type, IReadOnlyDictionary<string, FieldInfo>> GetBackingFields(IModel model)
    {
        var dictionary = new Dictionary<Type, IReadOnlyDictionary<string, FieldInfo>>();
        foreach (var entityType in model.GetEntityTypes())
        {
            if (dictionary.ContainsKey(entityType.ClrType))
            {
                continue;
            }

            Dictionary<string, FieldInfo>? fields = null;
            foreach (var property in entityType.GetProperties())
            {
                if (property.PropertyInfo is { CanWrite: false } propertyInfo &&
                    property.FieldInfo is { } field &&
                    field.FieldType == propertyInfo.PropertyType)
                {
                    fields ??= new(StringComparer.OrdinalIgnoreCase);
                    fields[propertyInfo.Name] = field;
                }
            }

            if (fields is not null)
            {
                dictionary[entityType.ClrType] = fields;
            }
        }

        return dictionary;
    }
}
