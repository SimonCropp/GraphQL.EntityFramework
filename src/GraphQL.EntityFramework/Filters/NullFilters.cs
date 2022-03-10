using GraphQL.EntityFramework;

class NullFilters :
    Filters
{
    public static NullFilters Instance = new();

    internal override Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(IEnumerable<TEntity> result, object userContext) =>
        Task.FromResult(result);

    internal override Task<bool> ShouldInclude<TEntity>(object userContext, TEntity? item)
        where TEntity : class =>
        Task.FromResult(true);
}