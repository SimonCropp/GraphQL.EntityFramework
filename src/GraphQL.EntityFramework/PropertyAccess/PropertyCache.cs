/// <summary>
/// The property cache for a type known only at runtime, for the orderBy of a navigation applied
/// inside its collection subquery.
/// </summary>
static class PropertyCache
{
    static ConcurrentDictionary<Type, MethodInfo> methods = new();

    public static IProperty GetProperty(Type type, string path)
    {
        var method = methods.GetOrAdd(
            type,
            _ => typeof(PropertyCache<>).MakeGenericType(_).GetMethod(nameof(PropertyCache<>.GetProperty), [typeof(string)])!);
        try
        {
            return (IProperty) method.Invoke(null, [path])!;
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}

static class PropertyCache<TInput>
{
    public static ParameterExpression SourceParameter = Expression.Parameter(typeof(TInput));
    static ConcurrentDictionary<string, Property<TInput>> properties = [];

    // Used by tests to assert the cache stays bounded.
    internal static int CachedCount => properties.Count;

    /// <summary>
    /// Paths arrive from the client, and a self referencing navigation can mint an unlimited number
    /// of distinct valid paths, so the cache cannot grow with them. On overflow the cache resets
    /// rather than evicting, which keeps memory bounded without eviction bookkeeping: under normal
    /// use the cap is never reached, and under a flood of distinct paths genuinely hot paths simply
    /// repopulate. Rebuilding is cheap now that the delegate is compiled lazily.
    /// </summary>
    const int maxCachedPaths = 1000;

    /// <summary>
    /// A deep path expands into a member access per segment, and on the queryable paths into a join
    /// per segment, so an unbounded depth lets a small request generate an enormous query.
    /// </summary>
    const int maxPathDepth = 10;

    public static Property<TInput> GetProperty(string path)
    {
        if (properties.TryGetValue(path, out var existing))
        {
            return existing;
        }

        var property = Build(path);

        if (properties.Count >= maxCachedPaths)
        {
            properties.Clear();
        }

        properties.TryAdd(path, property);

        return property;
    }

    static Property<TInput> Build(string path)
    {
        var left = AggregatePath(path, SourceParameter);

        var converted = Expression.Convert(left, typeof(object));
        var lambda = Expression.Lambda<Func<TInput, object>>(converted, SourceParameter);
        var listContains = ReflectionCache.GetListContains(left.Type);

        var body = (MemberExpression)left;
        return new(
            Left: left,
            Lambda: lambda,
            SourceParameter: SourceParameter,
            PropertyType: left.Type,
            Info: body.Member,
            ListContains: listContains
        );
    }

    static Expression AggregatePath(string path, Expression parameter)
    {
        var segments = path.Split('.');
        if (segments.Length > maxPathDepth)
        {
            throw new($"Path exceeds the maximum depth of {maxPathDepth}. Type: {typeof(TInput).FullName}, Path: {path}.");
        }

        try
        {
            return segments
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
        var propertyOrField = TryGetPropertyOrField(type, propertyOrFieldName);

        // If property is still empty
        if (propertyOrField is null)
        {
            // Property does not exist on current type
            throw new ArgumentException($"'{propertyOrFieldName}' is not a member of type {type.FullName}");
        }

        return propertyOrField;
    }

    static MemberInfo? TryGetPropertyOrField(Type type, string propertyOrFieldName)
    {
        // Member search binding flags.
        // Only public members are resolved. Paths arrive from the client, so falling back to
        // non public members would make anything the clr can read filterable, including members
        // deliberately kept out of the graph.
        const BindingFlags bindingFlagsPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;

        // Attempt to get the public property
        var propertyOrField = type.GetProperty(propertyOrFieldName, bindingFlagsPublic) ??
                              (MemberInfo?)type.GetField(propertyOrFieldName, bindingFlagsPublic);

        // If property/field was not resolved, search inherited interfaces.
        // Interface member lookup is not flattened, so each base interface must be
        // probed individually until a match is found.
        if (propertyOrField is null && type.IsInterface)
        {
            foreach (var baseInterfaceType in type.GetInterfaces())
            {
                propertyOrField = TryGetPropertyOrField(baseInterfaceType, propertyOrFieldName);
                if (propertyOrField is not null)
                {
                    break;
                }
            }
        }

        return propertyOrField;
    }
}
