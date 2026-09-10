namespace GraphQL.EntityFramework;

/// <summary>
/// The where for a collection navigation of <typeparamref name="TItem"/>: any, all, or none of
/// the items match.
/// </summary>
public class CollectionWhereGraph<TItem> :
    InputObjectGraphType,
    IWhereGraph
{
    public CollectionWhereGraph()
    {
        Name = GraphNames.CollectionWhere(typeof(TItem));
        var itemWhere = typeof(WhereGraph<TItem>);
        foreach (var name in new[] { "any", "all", "none" })
        {
            AddField(new()
            {
                Name = name,
                Type = itemWhere
            });
        }
    }

    public override object ParseDictionary(IDictionary<string, object?> value) =>
        value;
}
