static class KeyNameExtractor
{
    public static IReadOnlyDictionary<Type, List<string>> GetKeyNames(this IModel model)
    {
        var keyNames = new Dictionary<Type, List<string>>();
        foreach (var entity in model.GetEntityTypes())
        {
            // The join entity EF creates for a shadow many to many is a property bag, a
            // Dictionary<string, object>. It used to be skipped by the assembly name starting
            // with System, which also dropped every entity in a user assembly named that way.
            if (entity.IsPropertyBag)
            {
                continue;
            }

            var primaryKey = entity.FindPrimaryKey();
            //This can happen for views
            if (primaryKey is null)
            {
                continue;
            }

            if (entity.IsOwned())
            {
                continue;
            }

            var names = primaryKey.Properties.Select(_ => _.Name).ToList();
            keyNames.TryAdd(entity.ClrType, names);
        }

        return keyNames;
    }
}
