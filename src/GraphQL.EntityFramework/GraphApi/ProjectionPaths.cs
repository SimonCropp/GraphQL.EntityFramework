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

    public static ProjectionPaths Analyze(LambdaExpression projection) =>
        Analyze([projection]);

    /// <summary>
    /// The paths of every projection a field declares, in the order they were declared. A field
    /// can be given more than one projection, and each one's data has to be loaded.
    /// </summary>
    public static ProjectionPaths Analyze(IReadOnlyList<LambdaExpression> projections)
    {
        var groups = new List<ProjectionPathGroup>();
        var byRoot = new Dictionary<string, ProjectionPathGroup>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in projections.SelectMany(ProjectionAnalyzer.ExtractPropertyPaths))
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
        // Two projections on one field can read the same path.
        if (nested.Contains(path))
        {
            return;
        }

        nested.Add(path);
        if (!path.Contains('.'))
        {
            nestedScalars.Add(path);
        }
    }
}
