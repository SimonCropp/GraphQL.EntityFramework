using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata(nameof(OrderDetail))]
public class OrderDetailGraph :
    EfObjectGraphType<SampleDbContext, OrderDetail>
{
    public OrderDetailGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}