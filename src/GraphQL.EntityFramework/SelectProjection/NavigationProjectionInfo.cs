record NavigationProjectionInfo(
    Type EntityType,
    bool IsCollection,
    FieldProjectionInfo Projection)
{
    public NavigationProjectionInfo Merge(NavigationProjectionInfo other) =>
        this with
        {
            Projection = Projection.Merge(other.Projection)
        };
}