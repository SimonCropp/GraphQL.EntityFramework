using GraphQL.EntityFramework;

public class ManyToManyRightGraphType :
    EfObjectGraphType<IntegrationDbContext, ManyToManyRightEntity>
{
    public ManyToManyRightGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}