using GraphQL.EntityFramework;

public class MappingChildGraph :
    EfObjectGraphType<MappingContext, MappingChild>
{
    public MappingChildGraph(IEfGraphQLService<MappingContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationField(
            name: "parent",
            resolve: context => context.Source.Parent);
        AutoMap();
    }
}