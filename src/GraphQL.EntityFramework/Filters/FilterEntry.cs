class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
{
    Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    Func<object, TProjection>? compiledProjection;
    IReadOnlySet<string> requiredPropertyNames;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>>? projection)
    {
        this.filter = filter;
        if (projection is null)
        {
            compiledProjection = null;
            requiredPropertyNames = new HashSet<string>();
        }
        else
        {
            var compiled = projection.Compile();
            compiledProjection = entity => compiled((TEntity)entity);
            requiredPropertyNames = ProjectionAnalyzer.ExtractPropertyPaths(projection);
        }
    }

    public FieldProjectionInfo AddRequirements(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties)
    {
        if (requiredPropertyNames.Count == 0)
        {
            return projection;
        }

        var mergedScalars = new HashSet<string>(projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
        var navRequirements = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in requiredPropertyNames)
        {
            var dotIndex = field.IndexOf('.');
            if (dotIndex < 0)
            {
                // Scalar field - skip navigation property names
                if (FindNavigation(navigationProperties, field) == null)
                {
                    mergedScalars.Add(field);
                }

                continue;
            }

            var navProperty = field[(dotIndex + 1)..];

            // Only handle single-level navigation paths
            if (navProperty.Contains('.'))
            {
                continue;
            }

            var navName = field[..dotIndex];
            if (!navRequirements.TryGetValue(navName, out var props))
            {
                props = new(StringComparer.OrdinalIgnoreCase);
                navRequirements[navName] = props;
            }

            props.Add(navProperty);
        }

        // Merge navigation requirements
        var mergedNavigations = projection.Navigations != null
            ? new Dictionary<string, NavigationProjectionInfo>(projection.Navigations)
            : new Dictionary<string, NavigationProjectionInfo>();

        foreach (var (navName, requiredProps) in navRequirements)
        {
            var navMetadata = FindNavigation(navigationProperties, navName);
            if (navMetadata == null)
            {
                continue;
            }

            if (mergedNavigations.TryGetValue(navName, out var existingNav))
            {
                var updatedScalars = new HashSet<string>(existingNav.Projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
                updatedScalars.UnionWith(requiredProps);
                mergedNavigations[navName] = existingNav with
                {
                    Projection = existingNav.Projection with { ScalarFields = updatedScalars }
                };
            }
            else
            {
                mergedNavigations[navName] = new(navMetadata.Type, navMetadata.IsCollection, new(requiredProps, null, null, null));
            }
        }

        return projection with
        {
            ScalarFields = mergedScalars,
            Navigations = mergedNavigations
        };
    }

    static Navigation? FindNavigation(IReadOnlyDictionary<string, Navigation>? properties, string name)
    {
        if (properties == null)
        {
            return null;
        }

        foreach (var (key, value) in properties)
        {
            if (string.Equals(key, name, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

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
}
