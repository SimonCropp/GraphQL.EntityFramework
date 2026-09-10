public class TphAttachmentGraphType :
    EfObjectGraphType<IntegrationDbContext, TphAttachmentEntity>
{
    public TphAttachmentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap(["Request", "RelatedRequest"]);
        AddNavigationField(
            name: "relatedRequest",
            projection: _ => _.RelatedRequest,
            resolve: _ => _.Projection,
            graphType: typeof(TphMiddleGraphType));
    }
}
