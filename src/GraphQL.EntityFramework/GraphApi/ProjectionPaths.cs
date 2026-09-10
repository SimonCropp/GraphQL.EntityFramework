/// <summary>
/// The property paths a projection expression reads, grouped by the root property each starts
/// from, in the order the expression reads them. Analyzed once when the field is registered and
/// kept in the field metadata. It was analyzed again on every request that selected the field,
/// with a visitor walk over the expression and the grouping rebuilt each time.
/// </summary>
sealed class ProjectionPaths
{
    ProjectionPaths(IReadOnlyList<ProjectionPathGroup> groups) =>
        Groups = groups;

    public IReadOnlyList<ProjectionPathGroup> Groups { get; }

    /// <summary>
    /// The root of the first path read. It receives the field's selection set when the type the
    /// field returns cannot say which navigation the selection applies to.
    /// </summary>
    public string? PrimaryRoot =>
        Groups.Count == 0 ? null : Groups[0].Root;

    public static ProjectionPaths Analyze(LambdaExpression projection)
    {
        var groups = new List<ProjectionPathGroup>();
        var byRoot = new Dictionary<string, ProjectionPathGroup>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in ProjectionAnalyzer.ExtractPropertyPaths(projection))
        {
            var dotIndex = path.IndexOf('.');
            var root = dotIndex >= 0 ? path[..dotIndex] : path;

            if (!byRoot.TryGetValue(root, out var group))
            {
                group = new(root);
                byRoot[root] = group;
                groups.Add(group);
            }

            if (dotIndex >= 0)
            {
                group.Add(path[(dotIndex + 1)..]);
            }
        }

        return new(groups);
    }
}

/// <summary>
/// The paths read below one property of the entity.
/// </summary>
sealed class ProjectionPathGroup(string root)
{
    List<string> nested = [];
    List<string> nestedScalars = [];

    /// <summary>
    /// The property on the entity the paths start from.
    /// </summary>
    public string Root { get; } = root;

    /// <summary>
    /// The paths below <see cref="Root"/>, without it. Empty when the root was read whole.
    /// </summary>
    public IReadOnlyList<string> Nested => nested;

    /// <summary>
    /// The single segment paths in <see cref="Nested"/>: the scalars read from the navigation.
    /// </summary>
    public IReadOnlyList<string> NestedScalars => nestedScalars;

    internal void Add(string path)
    {
        nested.Add(path);
        if (!path.Contains('.'))
        {
            nestedScalars.Add(path);
        }
    }
}
