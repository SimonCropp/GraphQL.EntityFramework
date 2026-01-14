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

        #region ProjectedFieldBasicTransform

        AddProjectedNavigationField<ParentEntity, string?, string>(
            name: "propertyUpper",
            resolve: _ => _.Source,
            projection: entity => entity.Property,
            transform: property => property?.ToUpper() ?? "");

        #endregion

        #region ProjectedFieldAsyncTransform

        AddProjectedNavigationField<ParentEntity, string?, string>(
            name: "propertyUpperAsync",
            resolve: _ => _.Source,
            projection: entity => entity.Property,
            transform: async property =>
            {
                await Task.Yield();
                return property?.ToUpper() ?? "";
            });

        #endregion

        #region ProjectedFieldContextAwareTransform

        AddProjectedNavigationField<ParentEntity, string?, string>(
            name: "propertyWithContext",
            resolve: _ => _.Source,
            projection: entity => entity.Property,
            transform: (context, property) =>
            {
                var prefix = context.Source.Id.ToString()[..8];
                return $"{prefix}: {property ?? "null"}";
            });

        #endregion

        #region ProjectedFieldListField

        AddProjectedNavigationListField<ChildEntity, string?, string>(
            name: "childrenProperties",
            resolve: _ => _.Source.Children,
            projection: child => child.Property,
            transform: property => property ?? "empty",
            includeNames: ["Children"]);

        #endregion

        // Projected field with complex projection
        AddProjectedNavigationField<ParentEntity, string, string>(
            name: "anonymousProjection",
            resolve: _ => _.Source,
            projection: entity => entity.Id + "|" + (entity.Property ?? "null"),
            transform: data => data);

        AutoMap();
    }
}
