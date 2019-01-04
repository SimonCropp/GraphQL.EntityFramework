using GraphQL.EntityFramework;

public class CustomTypeGraph :
    EfObjectGraphType<CustomTypeEntity>
{
    public CustomTypeGraph(IEfGraphQLService graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}