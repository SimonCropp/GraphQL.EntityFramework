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

        // Most filters ask for fields the query already selected, so the copies are deferred
        // until a field is actually added. Otherwise every filter copied the projection per request.
        HashSet<string>? mergedScalars = null;
        Dictionary<string, HashSet<string>>? navRequirements = null;

        foreach (var field in requiredPropertyNames)
        {
            var dotIndex = field.IndexOf('.');
            if (dotIndex < 0)
            {
                // Scalar field - skip navigation property names
                if (FindNavigation(navigationProperties, field) == null &&
                    !projection.ScalarFields.Contains(field))
                {
                    mergedScalars ??= new(projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
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
            if (projection.Navigations is not null &&
                projection.Navigations.TryGetValue(navName, out var existing) &&
                existing.Projection.ScalarFields.Contains(navProperty))
            {
                continue;
            }

            navRequirements ??= new(StringComparer.OrdinalIgnoreCase);
            if (!navRequirements.TryGetValue(navName, out var props))
            {
                props = [with(StringComparer.OrdinalIgnoreCase)];
                navRequirements[navName] = props;
            }

            props.Add(navProperty);
        }

        if (mergedScalars is null && navRequirements is null)
        {
            return projection;
        }

        // Merge navigation requirements
        var mergedNavigations = projection.Navigations;
        if (navRequirements is not null)
        {
            mergedNavigations = mergedNavigations is null ? [] : new(mergedNavigations);
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
        }

        return projection with
        {
            ScalarFields = mergedScalars ?? projection.ScalarFields,
            Navigations = mergedNavigations
        };
    }

    // The navigation dictionaries are built with a case insensitive comparer, so a lookup
    // is enough. This was a linear scan comparing every key.
    static Navigation? FindNavigation(IReadOnlyDictionary<string, Navigation>? properties, string name)
    {
        if (properties is not null &&
            properties.TryGetValue(name, out var navigation))
        {
            return navigation;
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
