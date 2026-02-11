public class TphMiddleGraphType :
    EfInterfaceGraphType<IntegrationDbContext, TphMiddleEntity>
{
    public TphMiddleGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService, _ => _.Attachments) =>
        AddNavigationListField(
            name: "attachments",
            projection: _ => _.Attachments);
}
