/// <param name="IsWhole">
/// The whole entity is wanted, because a projection expression or filter read the navigation
/// itself rather than properties of it. The select projection then binds the navigation whole
/// instead of building a member init from <paramref name="Projection"/>.
/// </param>
/// <param name="IsRequired">
/// The navigation is required in the EF model, so is never null. The select projection then
/// skips the null check, which EF would otherwise translate to an always false comparison.
/// </param>
/// <param name="Arguments">
/// The field's ids, where and orderBy, to apply inside the collection subquery. Null when the
/// navigation was selected more than once, since one loaded collection cannot satisfy two sets
/// of arguments; the field's resolver then applies them in memory. One selection read through
/// the field of each type implementing an interface is still one selection, and keeps them.
/// </param>
record NavigationProjectionInfo(
    Type EntityType,
    bool IsCollection,
    FieldProjectionInfo Projection,
    bool IsWhole = false,
    NavigationArguments? Arguments = null,
    bool IsRequired = false)
{
    public NavigationProjectionInfo Merge(NavigationProjectionInfo other)
    {
        var sameSelection = Arguments is not null &&
                            other.Arguments is not null &&
                            ReferenceEquals(Arguments.Field, other.Arguments.Field);
        return this with
        {
            Projection = Projection.Merge(other.Projection),
            IsWhole = IsWhole || other.IsWhole,
            Arguments = sameSelection ? Arguments : null
        };
    }
}