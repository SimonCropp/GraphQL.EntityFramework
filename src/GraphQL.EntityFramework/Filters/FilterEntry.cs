class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
{
    Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    Func<object, TProjection>? compiledProjection;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>>? projection)
    {
        this.filter = filter;
        if (projection is null)
        {
            compiledProjection = null;
            RequiredPropertyNames = new HashSet<string>();
        }
        else
        {
            var compiled = projection.Compile();
            compiledProjection = entity => compiled((TEntity)entity);
            RequiredPropertyNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
            ValidateProjectionCompatibility(projection, RequiredPropertyNames);
        }
    }

    public IReadOnlySet<string> RequiredPropertyNames { get; }

    public Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity)
    {
        var projectedData = compiledProjection is not null
            ? compiledProjection(entity)
            : default!;
        return filter(userContext, data, userPrincipal, projectedData);
    }

    static void ValidateProjectionCompatibility(
        Expression<Func<TEntity, TProjection>> projection,
        IReadOnlySet<string> requiredPropertyNames)
    {
        // Only validate identity projections (x => x)
        // Explicit projections that extract specific properties from abstract navigations are ALLOWED
        // because EF Core can project scalar properties through abstract navigations efficiently.
        // The problem only occurs with identity projections where the filter accesses abstract nav properties,
        // which forces Include() to load all columns.
        if (!IsIdentityProjection(projection))
        {
            return;
        }

        // Extract navigation paths (e.g., "Parent.Property" -> navigation "Parent")
        var navigationPaths = requiredPropertyNames
            .Where(_ => _.Contains('.'))
            .Select(_ => _.Split('.')[0])
            .Distinct()
            .ToList();

        if (navigationPaths.Count == 0)
        {
            return;
        }

        var entityType = typeof(TEntity);

        // Check each navigation for abstract types
        foreach (var navPath in navigationPaths)
        {
            var navProperty = entityType.GetProperty(navPath);
            if (navProperty == null)
            {
                continue;
            }

            var navType = navProperty.PropertyType;

            // For collections, get the element type
            if (navType.IsGenericType)
            {
                var genericDef = navType.GetGenericTypeDefinition();
                if (genericDef == typeof(ICollection<>) ||
                    genericDef == typeof(IList<>) ||
                    genericDef == typeof(IEnumerable<>) ||
                    genericDef == typeof(List<>))
                {
                    navType = navType.GetGenericArguments()[0];
                }
            }

            if (navType.IsAbstract)
            {
                throw new InvalidOperationException(
                    $"Filter for '{entityType.Name}' uses identity projection '_ => _' " +
                    $"to access properties of abstract navigation '{navPath}' ({navType.Name}). " +
                    $"This forces Include() to load all columns from {navType.Name}. " +
                    $"Extract only the required properties in an explicit projection: " +
                    $"projection: e => new {{ e.Id, {navPath}Property = e.{navPath}.PropertyName }}, " +
                    $"filter: (_, _, _, proj) => proj.{navPath}Property == value");
            }
        }
    }

    // Detect: x => x (where body == parameter)
    static bool IsIdentityProjection(Expression<Func<TEntity, TProjection>> projection) =>
        projection.Body == projection.Parameters[0];
}
