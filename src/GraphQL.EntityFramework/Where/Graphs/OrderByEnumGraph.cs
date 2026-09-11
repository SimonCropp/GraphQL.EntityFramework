namespace GraphQL.EntityFramework;

/// <summary>
/// The orderBy enum for <typeparamref name="TEntity"/>. One value per orderable path, and a
/// <c>_desc</c> variant of each. Reference navigations are flattened, so Parent.Property is
/// <c>parent_property</c>, to the depth of <see cref="OrderByEnumOptions.NestingDepth"/>.
/// Since a single value coerces to a one item list, <c>orderBy: property</c> is the short form
/// of <c>orderBy: [property]</c>, and several keys are <c>orderBy: [property, id_desc]</c>.
/// </summary>
public class OrderByEnumGraph<TEntity> :
    EnumerationGraphType
{
    const string descendingSuffix = "_desc";

    public OrderByEnumGraph(IEnumerable<IEfGraphQLService> services, OrderByEnumOptions options)
    {
        var type = typeof(TEntity);
        Name = GraphNames.OrderBy(type);
        // The path each value was generated from, to report a collision against
        var sources = new Dictionary<string, string>(StringComparer.Ordinal);
        Add(type, services.ToList(), null, null, 1, options.NestingDepth, [type], sources);
        if (Values.Count == 0)
        {
            throw new($"The orderBy enum {Name} has no values, since {type.Name} exposes no orderable properties.");
        }
    }

    void Add(
        Type type,
        IReadOnlyList<IEfGraphQLService> services,
        string? namePrefix,
        string? pathPrefix,
        int depth,
        int maxDepth,
        HashSet<Type> visited,
        Dictionary<string, string> sources)
    {
        foreach (var member in EntityShape.Members(type, services))
        {
            if (member.Kind == MemberKind.Collection)
            {
                continue;
            }

            var memberName = member.Name.ToCamelCase();
            var name = namePrefix is null ? memberName : $"{namePrefix}_{memberName}";
            var path = pathPrefix is null ? member.Name : $"{pathPrefix}.{member.Name}";

            if (member.Kind == MemberKind.Scalar)
            {
                Add(name, path, false, sources);
                Add(name + descendingSuffix, path, true, sources);
                continue;
            }

            // A cycle, as in Parent.Child.Parent, would flatten forever
            if (depth >= maxDepth ||
                !visited.Add(member.Type))
            {
                continue;
            }

            Add(member.Type, services, name, path, depth + 1, maxDepth, visited, sources);
            visited.Remove(member.Type);
        }
    }

    void Add(string name, string path, bool descending, Dictionary<string, string> sources)
    {
        var source = descending ? $"{path} descending" : path;
        if (sources.TryGetValue(name, out var existing))
        {
            throw new($"The orderBy enum {Name} generated the value '{name}' for both '{existing}' and '{source}'. Flattening a navigation uses the same separator as a property name, so the two collide. Rename one of the properties, or drop the nesting depth.");
        }

        sources.Add(name, source);
        Add(
            name,
            new OrderBy
            {
                Path = path,
                Descending = descending
            });
    }
}
