using GraphQL.EntityFramework;

public class OrderDetailGraphType :
    EfObjectGraphType<SampleDbContext, OrderDetail>
{
    public OrderDetailGraphType(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService) =>
        AutoMap();
}