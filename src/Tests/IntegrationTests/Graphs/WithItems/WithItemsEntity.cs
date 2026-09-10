public class WithItemsEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public IList<ItemEntity> Items { get; set; } = [];
}

public class ItemEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Property { get; set; }
    public Guid? ParentId { get; set; }
    public WithItemsEntity? Parent { get; set; }
}

public class WithItemsGraphType :
    EfObjectGraphType<IntegrationDbContext, WithItemsEntity>
{
    public WithItemsGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}

public class ItemGraphType :
    EfObjectGraphType<IntegrationDbContext, ItemEntity>
{
    public ItemGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap(["Parent"]);
}
