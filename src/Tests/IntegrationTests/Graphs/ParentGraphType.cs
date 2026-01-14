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

        // Projected navigation field tests - simple transform
        AddProjectedNavigationField<ParentEntity, string?, string>(
            name: "propertyUpper",
            resolve: _ => _.Source,
            projection: entity => entity.Property,
            transform: property => property?.ToUpper() ?? "",
            includeNames: ["Property"]);

        // Projected navigation field tests - async transform
        AddProjectedNavigationField<ParentEntity, string?, string>(
            name: "propertyUpperAsync",
            resolve: _ => _.Source,
            projection: entity => entity.Property,
            transform: async property =>
            {
                await Task.Yield();
                return property?.ToUpper() ?? "";
            },
            includeNames: ["Property"]);

        // Projected navigation field tests - context-aware transform
        AddProjectedNavigationField<ParentEntity, string?, string>(
            name: "propertyWithContext",
            resolve: _ => _.Source,
            projection: entity => entity.Property,
            transform: (context, property) =>
            {
                var prefix = context.Source.Id.ToString()[..8];
                return $"{prefix}: {property ?? "null"}";
            },
            includeNames: ["Property"]);

        // Projected navigation list field tests
        AddProjectedNavigationListField<ChildEntity, string?, string>(
            name: "childrenProperties",
            resolve: _ => _.Source.Children,
            projection: child => child.Property,
            transform: property => property ?? "empty",
            includeNames: ["Children", "Children.Property"]);

        // Projected field with complex projection
        AddProjectedNavigationField<ParentEntity, string, string>(
            name: "anonymousProjection",
            resolve: _ => _.Source,
            projection: entity => entity.Id.ToString() + "|" + (entity.Property ?? "null"),
            transform: data => data,
            includeNames: ["Property"]);

        AutoMap();
    }
}