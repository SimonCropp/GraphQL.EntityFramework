public class ParentGraphType :
    EfObjectGraphType<IntegrationDbContext, ParentEntity>
{
    public ParentGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddNavigationConnectionField(
            name: "childrenConnection",
            resolve: _ => _.Source.Children,
            includeNames: [ "Children" ]);
        AddNavigationConnectionField(
            name: "childrenConnectionOmitQueryArguments",
            resolve: _ => _.Source.Children,
            includeNames: [ "Children" ],
            omitQueryArguments: true);
        AutoMap();
    }
}