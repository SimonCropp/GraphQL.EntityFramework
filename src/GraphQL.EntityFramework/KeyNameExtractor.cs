static class KeyNameExtractor
{
    public static IReadOnlyDictionary<Type, List<Key>> GetKeys(this IModel model)
    {
        var keyNames = new Dictionary<Type, List<Key>>();
        foreach (var entity in model.GetEntityTypes())
        {
            var clrType = entity.ClrType;

            // join entities ClrTypes are dictionaries
            if (clrType.Assembly.FullName!.StartsWith("System"))
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

            var names = primaryKey.Properties.Select(_ => new Key(_.Name,_.ClrType)).ToList();
            keyNames.Add(clrType, names);
        }

        return keyNames;
    }
}