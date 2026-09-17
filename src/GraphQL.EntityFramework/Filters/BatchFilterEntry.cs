class BatchFilterEntry<TDbContext, TEntity, TProjection> :
    IBatchFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
{
    Filters<TDbContext>.BatchFilter<TProjection> filter;
    Func<TEntity, TProjection> compiledProjection;

    // A batch filter loads what a per item filter on the same projection loads, so the
    // requirements come from one of those rather than a second copy of that logic
    FilterEntry<TDbContext, TEntity, TProjection> requirements;

    public BatchFilterEntry(
        Filters<TDbContext>.BatchFilter<TProjection> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        compiledProjection = projection.Compile();
        requirements = new((_, _, _, _) => Task.FromResult(true), projection);
    }

    public FieldProjectionInfo AddRequirements(
        FieldProjectionInfo projection,
        IReadOnlyDictionary<string, Navigation>? navigationProperties) =>
        requirements.AddRequirements(projection, navigationProperties);

    public object? Project(object entity) =>
        compiledProjection((TEntity)entity);

    public async Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity)
    {
        var projection = Project(entity);
        var included = await Filter(userContext, data, userPrincipal, [projection]);
        return included(projection);
    }

    public async Task<Func<object?, bool>> Filter(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        IReadOnlyCollection<object?> projections)
    {
        var inputs = projections
            .Select(_ => (TProjection)_!)
            .ToList();

        IReadOnlySet<TProjection> included;
        try
        {
            included = await filter(userContext, data, userPrincipal, inputs);
        }
        catch (Exception exception)
        {
            throw new($"Failed to execute batch filter. {nameof(TEntity)}: {typeof(TEntity)}.", exception);
        }

        return _ => included.Contains((TProjection)_!);
    }
}
