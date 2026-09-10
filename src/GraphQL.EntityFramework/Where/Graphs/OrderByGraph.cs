namespace GraphQL.EntityFramework;

/// <summary>
/// The orderBy input type for <typeparamref name="TEntity"/>. One field per mapped property
/// taking a <see cref="SortDirection"/>, and one per reference navigation to order by a
/// nested property. Each item of the orderBy list sets exactly one field, since the order of
/// fields inside an object is not something a client can rely on.
/// </summary>
public class OrderByGraph<TEntity> :
    InputObjectGraphType
{
    const string memberKey = "EfMember";

    public OrderByGraph(IEnumerable<IEfGraphQLService> services)
    {
        var type = typeof(TEntity);
        Name = GraphNames.OrderBy(type);
        foreach (var member in EntityShape.Members(type, services))
        {
            if (member.Kind == MemberKind.Collection)
            {
                continue;
            }

            var field = new FieldType
            {
                Name = member.Name,
                Type = member.Kind == MemberKind.Scalar
                    ? typeof(SortDirectionGraph)
                    : typeof(OrderByGraph<>).MakeGenericType(member.Type)
            };
            field.Metadata[memberKey] = member;
            AddField(field);
        }
    }

    public override object ParseDictionary(IDictionary<string, object?> value)
    {
        var entries = value
            .Where(_ => _.Value is not null)
            .ToList();
        if (entries.Count == 0)
        {
            throw new ExecutionError($"An orderBy item must set exactly one property. The {Name} set none.");
        }

        if (entries.Count > 1)
        {
            throw new ExecutionError($"An orderBy item must set exactly one property. The {Name} set {string.Join(", ", entries.Select(_ => _.Key))}. Use one item per property.");
        }

        var (key, raw) = entries[0];
        var member = (EntityMember) GetField(key)!.Metadata[memberKey]!;
        if (member.Kind == MemberKind.Scalar)
        {
            return new OrderBy
            {
                Path = member.Name,
                Descending = raw is SortDirection.Descending
            };
        }

        var nested = (OrderBy) raw!;
        return new OrderBy
        {
            Path = $"{member.Name}.{nested.Path}",
            Descending = nested.Descending
        };
    }
}
