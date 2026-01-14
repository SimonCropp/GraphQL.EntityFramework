public class Level1GraphType :
    EfObjectGraphType<IntegrationDbContext, Level1Entity>
{
    public Level1GraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        // Projected navigation field test - nested navigation
        AddProjectedNavigationField<Level2Entity, string?, string>(
            name: "level2Property",
            resolve: _ => _.Source.Level2Entity,
            projection: level2 => level2.Level3Entity!.Property,
            transform: property => property ?? "none",
            includeNames: ["Level2Entity", "Level2Entity.Level3Entity", "Level2Entity.Level3Entity.Property"]);

        AutoMap();
    }
}