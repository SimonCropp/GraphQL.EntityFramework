/// <param name="IsWhole">
/// The whole entity is wanted, because a projection expression or filter read the navigation
/// itself rather than properties of it. The select projection then binds the navigation whole
/// instead of building a member init from <paramref name="Projection"/>.
/// </param>
/// <param name="Arguments">
/// The field's ids, where and orderBy, to apply inside the collection subquery. Null when the
/// navigation was selected more than once, since one loaded collection cannot satisfy two sets
/// of arguments; the field's resolver then applies them in memory.
/// </param>
record NavigationProjectionInfo(
    Type EntityType,
    bool IsCollection,
    FieldProjectionInfo Projection,
    bool IsWhole = false,
    NavigationArguments? Arguments = null)
{
    public NavigationProjectionInfo Merge(NavigationProjectionInfo other) =>
        this with
        {
            Projection = Projection.Merge(other.Projection),
            IsWhole = IsWhole || other.IsWhole,
            Arguments = null
        };
}