class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
    where TProjection : class
{
    Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    Func<object, object> compiledProjection;
    IReadOnlySet<string> requiredProperties;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        var compiled = projection.Compile();
        compiledProjection = entity => compiled((TEntity) entity);
        requiredProperties = FilterProjectionAnalyzer.ExtractRequiredProperties(projection);
    }

    public Type EntityType => typeof(TEntity);

    public IReadOnlySet<string> GetRequiredPropertyNames() =>
        requiredProperties;

    public Func<object, object> GetCompiledProjection() =>
        compiledProjection;

    public Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity)
    {
        var projectedData = (TProjection) compiledProjection(entity);
        return filter(userContext, data, userPrincipal, projectedData);
    }
}
