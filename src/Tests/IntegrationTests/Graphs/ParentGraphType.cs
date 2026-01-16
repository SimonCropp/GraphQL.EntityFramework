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

        AddProjectedField<string?, string>(
            name: "propertyUpper",
            projection: source => source.Property,
            transform: property => property?.ToUpper() ?? "");

        #endregion

        #region ProjectedFieldAsyncTransform

        AddProjectedField<string?, string>(
            name: "propertyUpperAsync",
            projection: source => source.Property,
            transform: async property =>
            {
                await Task.Yield();
                return property?.ToUpper() ?? "";
            });

        #endregion

        // Projected field with complex projection
        AddProjectedField<string, string>(
            name: "anonymousProjection",
            projection: source => source.Id + "|" + (source.Property ?? "null"),
            transform: data => data);

        #region ProjectedFieldListField

        AddProjectedListField<string?, string>(
            name: "childrenProperties",
            projection: source => source.Children.Select(c => c.Property),
            transform: property => property ?? "empty");

        #endregion

        #region Projected Connection Field

        AddProjectedConnectionField<string?, string>(
            name: "childrenConnectionProjected",
            projection: source => source.Children.Select(c => c.Property),
            transform: prop => prop?.ToUpper() ?? "EMPTY");

        AddProjectedConnectionField<string?, string>(
            name: "childrenConnectionProjectedAsync",
            projection: source => source.Children.Select(c => c.Property),
            transform: async prop =>
            {
                await Task.Yield();
                return prop?.ToUpper() ?? "EMPTY";
            });

        #endregion

        // Single child's property
        AddProjectedField<string?, string>(
            name: "simplifiedFirstChildProperty",
            projection: source => source.Children.Select(c => c.Property).FirstOrDefault(),
            transform: prop => prop?.ToUpper() ?? "NO_CHILDREN");

        // List of child properties via projection
        AddProjectedListField<string?, string>(
            name: "simplifiedChildProperties",
            projection: source => source.Children.Select(c => c.Property),
            transform: prop => prop?.ToUpper() ?? "EMPTY");

        // Connection of child properties via projection
        AddProjectedConnectionField<string?, string>(
            name: "simplifiedChildrenConnection",
            projection: source => source.Children.Select(c => c.Property),
            transform: prop => prop?.ToUpper() ?? "EMPTY");

        AutoMap();
    }
}
