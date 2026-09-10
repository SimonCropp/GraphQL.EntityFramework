/// <param name="IsWhole">
/// The whole entity is wanted, because a projection expression or filter read the navigation
/// itself rather than properties of it. The select projection then binds the navigation whole
/// instead of building a member init from <paramref name="Projection"/>.
/// </param>
record NavigationProjectionInfo(
    Type EntityType,
    bool IsCollection,
    FieldProjectionInfo Projection,
    bool IsWhole = false)
{
    public NavigationProjectionInfo Merge(NavigationProjectionInfo other) =>
        this with
        {
            Projection = Projection.Merge(other.Projection),
            IsWhole = IsWhole || other.IsWhole
        };
}