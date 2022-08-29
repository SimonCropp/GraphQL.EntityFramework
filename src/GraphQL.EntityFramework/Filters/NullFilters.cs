using System.Security.Claims;

class NullFilters :
    Filters
{
    public static NullFilters Instance = new();

    internal override Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(IEnumerable<TEntity> result, object userContext, ClaimsPrincipal? userPrincipal) =>
        Task.FromResult(result);

    internal override Task<bool> ShouldInclude<TEntity>(object userContext, ClaimsPrincipal? userPrincipal, TEntity? item)
        where TEntity : class =>
        Task.FromResult(true);
}