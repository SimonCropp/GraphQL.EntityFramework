using GraphQL.EntityFramework;

public class CustomTypeGraph :
    EfObjectGraphType<MyDbContext, CustomTypeEntity>
{
    public CustomTypeGraph(IEfGraphQLService<MyDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        Field(x => x.Property);
    }
}