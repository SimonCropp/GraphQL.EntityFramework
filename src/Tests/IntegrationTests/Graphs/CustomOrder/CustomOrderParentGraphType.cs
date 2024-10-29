public class CustomOrderParentGraphType :
    EfObjectGraphType<IntegrationDbContext, CustomOrderParentEntity>
{
    public CustomOrderParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddQueryField(
            name: "customOrderChildren",
            resolve: context =>
            {
                var parentId = context.Source.Id;
                return context.DbContext.CustomOrderChildEntities
                    .Where(_ => _.ParentId == parentId);
            });
        AutoMap();
    }
}