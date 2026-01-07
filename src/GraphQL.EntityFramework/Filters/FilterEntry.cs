class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
{
    Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    Func<object, TProjection> compiledProjection;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        var compiled = projection.Compile();
        compiledProjection = entity => compiled((TEntity) entity);
        RequiredPropertyNames = FilterProjectionAnalyzer.ExtractRequiredProperties(projection);
    }

    public IReadOnlySet<string> RequiredPropertyNames { get; }

    public Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity)
    {
        var projectedData = compiledProjection(entity);
        return filter(userContext, data, userPrincipal, projectedData);
    }
}
