interface IFilterEntry<TDbContext>
    where TDbContext : DbContext
{
    Type EntityType { get; }
    Task<Dictionary<object, object>> QueryProjectedData(IEnumerable<object> entities, TDbContext data);
    Task<object?> QueryProjectedDataForSingle(object entity, TDbContext data);
    Task<bool> ShouldInclude(object userContext, TDbContext data, ClaimsPrincipal? userPrincipal, object item, Dictionary<(Type, object), object> projectedDataMap);
}
