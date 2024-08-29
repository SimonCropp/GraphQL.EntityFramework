static class KeyNameExtractor
{
    public static IReadOnlyDictionary<Type, List<string>> GetKeyNames(this IModel model)
    {
        var keyNames = new Dictionary<Type, List<string>>();
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

            var names = primaryKey.Properties.Select(_ => _.Name).ToList();
            keyNames.Add(clrType, names);
        }

        return keyNames;
    }
}