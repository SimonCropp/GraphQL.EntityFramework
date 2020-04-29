using GraphQL.EntityFramework;

public class MappingEntity1Graph :
    EfObjectGraphType<MappingContext, MappingEntity1>
{
    public MappingEntity1Graph(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}