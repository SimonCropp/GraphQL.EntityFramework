static class PropertyCache<TInput>
{
    public static ParameterExpression SourceParameter = Expression.Parameter(typeof(TInput));
    static ConcurrentDictionary<string, Property<TInput>> properties = [];

    public static Property<TInput> GetProperty(string path) =>
        properties.GetOrAdd(
            path,
            _ =>
            {
                var left = AggregatePath(_, SourceParameter);

                var converted = Expression.Convert(left, typeof(object));
                var lambda = Expression.Lambda<Func<TInput, object>>(converted, SourceParameter);
                var compile = lambda.Compile();
                var listContains = ReflectionCache.GetListContains(left.Type);

                var body = (MemberExpression)left;
                return new(
                    Left: left,
                    Lambda: lambda,
                    SourceParameter: SourceParameter,
                    Func: compile,
                    PropertyType: left.Type,
                    Info: body.Member,
                    ListContains: listContains
                );
            });

    static Expression AggregatePath(string path, Expression parameter)
    {
        try
        {
            return path.Split('.')
                .Aggregate(parameter, (current, property) =>
                    Expression.MakeMemberAccess(current, GetPropertyOrField(current.Type, property)));
        }
        catch (ArgumentException exception)
        {
            throw new($"Failed to create a member expression. Type: {typeof(TInput).FullName}, Path: {path}. Error: {exception.Message}");
        }
    }

    /// <summary>
    /// Get Specified property or field member info of provided type
    /// </summary>
    /// <param name="type">Type to retrieve property from</param>
    /// <param name="propertyOrFieldName">Name of property or field</param>
    static MemberInfo GetPropertyOrField(Type type, string propertyOrFieldName)
    {
        // Member search binding flags
        const BindingFlags bindingFlagsPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;
        const BindingFlags bindingFlagsNonPublic = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;

        // Attempt to get the public property
        var propertyOrField = type.GetProperty(propertyOrFieldName, bindingFlagsPublic) ??
                              (MemberInfo?)type.GetField(propertyOrFieldName, bindingFlagsPublic);

        // If not found
        propertyOrField ??= type.GetProperty(propertyOrFieldName, bindingFlagsNonPublic);

        // If not found
        propertyOrField ??= type.GetField(propertyOrFieldName, bindingFlagsNonPublic);

        // If property/ field was not resolved
        if (propertyOrField == null && type.IsInterface)
        {
            // Get All the implemented interfaces of the type
            var baseInterfaces = new List<Type>(type.GetInterfaces());

            // Iterate over inherited interfaces
            foreach (var baseInterfaceType in baseInterfaces)
            {
                // Recurse looking in the parent interface for the property
                propertyOrField = GetPropertyOrField(baseInterfaceType, propertyOrFieldName);

                // property found
                break;
            }
        }

        // If property is still empty
        if (propertyOrField is null)
        {
            // Property does not exist on current type
            throw new ArgumentException($"'{propertyOrFieldName}' is not a member of type {type.FullName}");
        }

        return propertyOrField;
    }
}