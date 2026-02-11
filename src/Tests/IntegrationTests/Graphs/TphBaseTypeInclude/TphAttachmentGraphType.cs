public class TphAttachmentGraphType :
    EfObjectGraphType<IntegrationDbContext, TphAttachmentEntity>
{
    public TphAttachmentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap(["Request"]);
}
