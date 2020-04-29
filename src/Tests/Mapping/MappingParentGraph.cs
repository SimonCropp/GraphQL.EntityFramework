using GraphQL.EntityFramework;

public class MappingParentGraph :
    EfObjectGraphType<MappingContext, MappingParent>
{
    public MappingParentGraph(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}