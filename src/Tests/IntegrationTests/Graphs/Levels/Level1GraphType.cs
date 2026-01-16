public class Level1GraphType :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public Level1GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        #region ProjectedFieldNestedNavigation

        AddProjectedField<string?, string>(
            name: "level2Property",
            projection: source => source.Level2Entity!.Level3Entity!.Property,
            transform: property => property ?? "none");

        #endregion

        AutoMap();
    }
}