using GraphQL.EntityFramework;

public class MappingSchema :
    GraphQL.Types.Schema
{
    public MappingSchema(EfGraphQLService<MappingContext> graphQlService, IServiceProvider provider) :
        base(provider)
    {
        Query = new MappingQuery(graphQlService);
        RegisterTypeMapping(typeof(MappingParent), typeof(MappingParentGraphType));
        RegisterTypeMapping(typeof(MappingChild), typeof(MappingChildGraphType));
    }
}