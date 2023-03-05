public class WithMisNamedQueryParentGraphType :
    EfObjectGraphType<IntegrationDbContext, WithMisNamedQueryParentEntity>
{
    public WithMisNamedQueryParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddQueryField(
            name: "misNamedChildren",
            resolve: context =>
            {
                var parentId = context.Source.Id;
                return context.DbContext.WithMisNamedQueryChildEntities
                    .Where(_ => _.ParentId == parentId);
            });
        AutoMap();
    }
}