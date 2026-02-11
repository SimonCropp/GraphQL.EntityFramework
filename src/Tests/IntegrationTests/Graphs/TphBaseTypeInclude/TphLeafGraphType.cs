public class TphLeafGraphType :
    EfObjectGraphType<IntegrationDbContext, TphLeafEntity>
{
    public TphLeafGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AutoMap();
        Interface<TphMiddleGraphType>();
        IsTypeOf = obj => obj is TphLeafEntity;
    }
}
