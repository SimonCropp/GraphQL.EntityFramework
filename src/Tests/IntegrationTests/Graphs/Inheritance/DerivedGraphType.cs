public class DerivedGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedEntity>
{
    public DerivedGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            _ => _.Source.ChildrenFromBase);
        AutoMap();
        Interface<InterfaceGraphType>();
        IsTypeOf = obj => obj is DerivedEntity;
    }
}