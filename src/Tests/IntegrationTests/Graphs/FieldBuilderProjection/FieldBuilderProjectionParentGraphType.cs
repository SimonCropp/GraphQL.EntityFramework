public class FieldBuilderProjectionParentGraphType :
    EfObjectGraphType<IntegrationDbContext, FieldBuilderProjectionParentEntity>
{
    public FieldBuilderProjectionParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "children",
            projection: _ => _.Children,
            resolve: _ => _.Projection);
        AutoMap();
    }
}
