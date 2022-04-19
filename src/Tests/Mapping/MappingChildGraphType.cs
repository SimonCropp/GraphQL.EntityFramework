using GraphQL.EntityFramework;

public class MappingChildGraphType :
    EfObjectGraphType<MappingContext, MappingChild>
{
    public MappingChildGraphType(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}