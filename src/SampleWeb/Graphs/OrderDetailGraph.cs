using GraphQL.EntityFramework;

public class OrderDetailGraph :
    EfObjectGraphType<SampleDbContext, OrderDetail>
{
    public OrderDetailGraph(IEfGraphQLService<SampleDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
    }
}