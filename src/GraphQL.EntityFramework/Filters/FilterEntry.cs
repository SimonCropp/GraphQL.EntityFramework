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
            requiredPropertyNames = ProjectionAnalyzer.ExtractRequiredProperties(projection);
            ValidateProjectionCompatibility(projection, requiredPropertyNames);
        }
    }

    public IReadOnlySet<string> RequiredPropertyNames => requiredPropertyNames;

    public FieldProjectionInfo AddRequirements(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties)
    {
        if (requiredPropertyNames.Count == 0)
        {
            return projection;
        }

        // Separate simple fields and navigation paths
        var scalarFieldsToAdd = new List<string>();
        var navigationPaths = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in requiredPropertyNames)
        {
            if (field.Contains('.'))
            {
                // Navigation path like "Parent.Id"
                var parts = field.Split('.', 2);
                var navName = parts[0];
                var navProperty = parts[1];

                if (!navigationPaths.TryGetValue(navName, out var properties))
                {
                    properties = new(StringComparer.OrdinalIgnoreCase);
                    navigationPaths[navName] = properties;
                }

                // Only add if it doesn't contain further dots (single-level navigation)
                if (!navProperty.Contains('.'))
                {
                    properties.Add(navProperty);
                }
            }
            else
            {
                // Simple field - check if it's a navigation property
                var isNavigation = navigationProperties?.ContainsKey(field) == true;

                if (isNavigation ||
                    projection.ScalarFields.Contains(field) ||
                    projection.KeyNames?.Contains(field, StringComparer.OrdinalIgnoreCase) == true)
                {
                    // Skip navigation names - they'll be handled via navigation paths
                    continue;
                }

                scalarFieldsToAdd.Add(field);
            }
        }

        // Merge scalar fields
        var mergedScalars = new HashSet<string>(projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
        foreach (var field in scalarFieldsToAdd)
        {
            mergedScalars.Add(field);
        }

        // Merge navigations
        var infos = projection.Navigations;
        Dictionary<string, NavigationProjectionInfo> mergedNavigations;
        if (infos == null)
        {
            mergedNavigations = [];
        }
        else
        {
            mergedNavigations = new(infos);
        }

        // Process navigation paths from filter fields
        foreach (var (navName, requiredProps) in navigationPaths)
        {
            // Skip if no navigation metadata available for this entity type
            if (navigationProperties == null)
            {
                continue;
            }

            // Try to find the navigation - use case-insensitive search
            Navigation? navMetadata = null;
            foreach (var (key, value) in navigationProperties)
            {
                if (string.Equals(key, navName, StringComparison.OrdinalIgnoreCase))
                {
                    navMetadata = value;
                    break;
                }
            }

            if (navMetadata == null)
            {
                continue;
            }

            var navType = navMetadata.Type;
            if (mergedNavigations.TryGetValue(navName, out var existingNav))
            {
                // Navigation exists in GraphQL query - add filter-required properties to its projection
                var updatedScalars = new HashSet<string>(existingNav.Projection.ScalarFields, StringComparer.OrdinalIgnoreCase);
                foreach (var prop in requiredProps)
                {
                    updatedScalars.Add(prop);
                }

                var updatedProjection = existingNav.Projection with
                {
                    ScalarFields = updatedScalars
                };
                mergedNavigations[navName] = existingNav with
                {
                    Projection = updatedProjection
                };
            }
            else
            {
                // Create navigation projection for filter-only navigations
                // Note: For abstract types, SelectExpressionBuilder.TryBuild will return false,
                // causing the entire projection to fail. This is intentional - it ensures
                // Include (added in AddFilterNavigationIncludes) is used instead of Select.
                // Don't include key/FK columns for filter-only navigations - the filter only
                // needs the specific properties it accesses.
                var navProjection = new FieldProjectionInfo(requiredProps, null, null, null);
                mergedNavigations[navName] = new(navType, navMetadata.IsCollection, navProjection);
            }
        }

        return projection with
        {
            ScalarFields = mergedScalars,
            Navigations = mergedNavigations
        };
    }

    public IEnumerable<string> GetAbstractNavigationIncludes(
        IReadOnlyDictionary<string, Navigation>? navigationProperties)
    {
        if (navigationProperties == null)
        {
            yield break;
        }

        // Extract navigation names from filter fields (paths like "Parent.Property" -> "Parent")
        var navigationNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in requiredPropertyNames)
        {
            if (field.Contains('.'))
            {
                var navName = field[..field.IndexOf('.')];
                navigationNames.Add(navName);
            }
        }

        // Return only navigations that have abstract types
        foreach (var navName in navigationNames)
        {
            // Find navigation in metadata (case-insensitive)
            Navigation? navMetadata = null;
            string? actualNavName = null;
            foreach (var (key, value) in navigationProperties)
            {
                if (string.Equals(key, navName, StringComparison.OrdinalIgnoreCase))
                {
                    navMetadata = value;
                    actualNavName = key;
                    break;
                }
            }

            if (navMetadata == null || actualNavName == null)
            {
                continue;
            }

            // Only return abstract types - concrete types can use projection
            if (navMetadata.Type.IsAbstract)
            {
                yield return navMetadata.Name;
            }
        }
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

            if (!navType.IsAbstract)
            {
                continue;
            }

            throw new(
                $$"""
                  Filter for '{{entityType.Name}}' uses identity projection '_ => _' to access properties of abstract navigation '{{navPath}}' ({{navType.Name}}).
                  This forces Include() to load all columns from {{navType.Name}}.
                  Extract only the required properties in an explicit projection:
                  projection: e => new { e.Id, {{navPath}}Property = e.{{navPath}}.PropertyName }, filter: (_, _, _, proj) => proj.{{navPath}}Property == value
                  """);
        }
    }

    // Detect: x => x (where body == parameter)
    static bool IsIdentityProjection(Expression<Func<TEntity, TProjection>> projection) =>
        projection.Body == projection.Parameters[0];
}
