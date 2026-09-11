namespace GraphQL.EntityFramework;

/// <summary>
/// The shape of the generated orderBy argument.
/// </summary>
public enum OrderByStyle
{
    /// <summary>
    /// An enum per entity, one value per orderable path, with a <c>_desc</c> variant of each. A single
    /// value coerces to a one item list, so a single key is <c>orderBy: property</c>, and several are
    /// <c>orderBy: [property, parent_property_desc]</c>. The default.
    /// </summary>
    Enum,

    /// <summary>
    /// An input object per entity, one field per member, taking a SortDirection:
    /// <c>orderBy: {property: ascending}</c> and <c>orderBy: {parent: {property: descending}}</c>.
    /// Each item of the list sets exactly one field.
    /// </summary>
    Object
}

/// <summary>
/// Options for <see cref="OrderByStyle.Enum"/>.
/// </summary>
public class OrderByEnumOptions
{
    /// <summary>
    /// How many path segments an orderBy value may have. 1 is the entity's own properties, 2 also
    /// flattens one reference navigation, as in <c>parent_property</c>. Each extra level multiplies
    /// the number of enum values, so the default is 2.
    /// </summary>
    public int NestingDepth { get; init; } = 2;
}
