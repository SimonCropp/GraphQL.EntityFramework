interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    Type EntityType { get; }

    IReadOnlySet<string> GetRequiredPropertyNames();

    Func<object, object> GetCompiledProjection();

    Task<bool> ShouldIncludeWithProjection(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object entity);
}
