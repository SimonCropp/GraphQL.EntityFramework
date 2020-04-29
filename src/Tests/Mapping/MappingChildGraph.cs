using GraphQL.EntityFramework;

public class MappingChildGraph :
    EfObjectGraphType<MappingContext, MappingChild>
{
    public MappingChildGraph(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}