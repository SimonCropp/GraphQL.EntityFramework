using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("WithNullable")]
public class WithNullableGraph :
    EfObjectGraphType<IntegrationDbContext, WithNullableEntity>
{
    public WithNullableGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}