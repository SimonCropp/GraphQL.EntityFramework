interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    IReadOnlySet<string> RequiredPropertyNames { get; }

    Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity);
}
