public class MappingParentGraphType :
    EfObjectGraphType<MappingContext, MappingParent>
{
    public MappingParentGraphType(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}