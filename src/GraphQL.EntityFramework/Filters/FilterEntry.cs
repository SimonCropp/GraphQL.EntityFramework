class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
    where TProjection : class
{
    readonly Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    readonly Expression<Func<TEntity, TProjection>> projection;

    IReadOnlySet<string>? requiredProperties;
    Func<TEntity, TProjection>? compiledProjection;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        this.projection = projection;
    }

    public Type EntityType => typeof(TEntity);

    public IReadOnlySet<string> GetRequiredPropertyNames()
    {
        requiredProperties ??= FilterProjectionAnalyzer.ExtractRequiredProperties(projection);
        return requiredProperties;
    }

    public Func<object, object> GetCompiledProjection()
    {
        compiledProjection ??= projection.Compile();
        return entity => compiledProjection((TEntity) entity);
    }

    public Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity)
    {
        compiledProjection ??= projection.Compile();
        var projectedData = compiledProjection((TEntity) entity);
        return filter(userContext, data, userPrincipal, projectedData);
    }
}
