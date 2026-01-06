namespace GraphQL.EntityFramework;

class FilterEntry<TDbContext, TEntity, TProjection> : IFilterEntry<TDbContext>
    where TDbContext : DbContext
    where TEntity : class
    where TProjection : class
{
    readonly Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter;
    readonly Expression<Func<TEntity, TProjection>> projection;
    readonly Lazy<Func<TEntity, TProjection>> compiledProjection;

    public FilterEntry(
        Func<object, TDbContext, ClaimsPrincipal?, TProjection, Task<bool>> filter,
        Expression<Func<TEntity, TProjection>> projection)
    {
        this.filter = filter;
        this.projection = projection;
        this.compiledProjection = new Lazy<Func<TEntity, TProjection>>(() => projection.Compile());
    }

    public Type EntityType => typeof(TEntity);
    public Type ProjectionType => typeof(TProjection);

    public LambdaExpression GetProjectionExpression() => projection;

    public object ProjectInMemory(object entity) =>
        compiledProjection.Value((TEntity) entity);

    public async Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object projectedData) =>
        await filter(userContext, data, userPrincipal, (TProjection) projectedData);
}
