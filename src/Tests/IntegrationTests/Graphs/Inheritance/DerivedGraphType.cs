public class DerivedGraphType :
    EfObjectGraphType<IntegrationDbContext, DerivedEntity>
{
    public DerivedGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenFromInterface",
            projection: _ => _.ChildrenFromBase,
            resolve: _ => _.Projection);
        Field<StringGraphType>("statusSummary")
            .Resolve(_ => $"Status is {_.Source.Status}")
            .WithProjection(_ => _.Status);
        AutoMap();
        Interface<BaseGraphType>();
        IsTypeOf = obj => obj is DerivedEntity;
    }
}
