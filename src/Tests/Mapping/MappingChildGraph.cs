using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(MappingChild))]
public class MappingChildGraph :
    EfObjectGraphType<MappingContext, MappingChild>
{
    public MappingChildGraph(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}