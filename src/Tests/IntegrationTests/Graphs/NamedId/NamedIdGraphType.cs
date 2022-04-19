using GraphQL.EntityFramework;

public class NamedIdGraphType :
    EfObjectGraphType<IntegrationDbContext, NamedIdEntity>
{
    public NamedIdGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}