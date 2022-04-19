using GraphQL.EntityFramework;

public class DerivedChildGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedChildEntity>
{
    public DerivedChildGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap(new List<string> { "Parent", "TypedParent" });
}