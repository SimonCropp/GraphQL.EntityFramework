namespace GraphQL.EntityFramework;

interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    Type EntityType { get; }
    Type ProjectionType { get; }
    LambdaExpression GetProjectionExpression();
    object ProjectInMemory(object entity);
    Task<bool> ShouldInclude(
        object userContext,
        TDbContext data,
        ClaimsPrincipal? userPrincipal,
        object projectedData);
}
