public class FieldBuilderProjectionGraphType :
    EfObjectGraphType<IntegrationDbContext, FieldBuilderProjectionEntity>
{
    public FieldBuilderProjectionGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        // Scalar property transformations use regular Resolve() - no projection needed
        Field<NonNullGraphType<IntGraphType>, int>("ageDoubled")
            .Resolve(_ => _.Source.Age * 2);

        Field<NonNullGraphType<BooleanGraphType>, bool>("isAdult")
            .Resolve(_ => _.Source.Age >= 18);

        Field<NonNullGraphType<DateTimeGraphType>, DateTime>("createdYear")
            .Resolve(_ => new(_.Source.CreatedAt.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Field<NonNullGraphType<DecimalGraphType>, decimal>("salaryWithBonus")
            .Resolve(_ => _.Source.Salary * 1.1m);

        Field<NonNullGraphType<FloatGraphType>, double>("scoreNormalized")
            .Resolve(_ => _.Source.Score / 100.0);

        Field<NonNullGraphType<LongGraphType>, long>("viewCountDoubled")
            .Resolve(_ => _.Source.ViewCount * 2);

        // Async operations can access PK/FK directly without projection
        Field<NonNullGraphType<BooleanGraphType>, bool>("hasParentAsync")
            .ResolveAsync(async ctx =>
            {
                await Task.Delay(1); // Simulate async operation
                var service = graphQlService.ResolveDbContext(ctx);
                return await service.Set<FieldBuilderProjectionParentEntity>()
                    .AnyAsync(_ => _.Children.Any(c => c.Id == ctx.Source.Id));
            });

        Field<NonNullGraphType<IntGraphType>, int>("siblingCountAsync")
            .ResolveAsync(async ctx =>
            {
                var dbContext = graphQlService.ResolveDbContext(ctx);
                var parent = await dbContext.Set<FieldBuilderProjectionParentEntity>()
                    .FirstOrDefaultAsync(_ => _.Children.Any(c => c.Id == ctx.Source.Id));
                return parent?.Children.Count ?? 0;
            });

        Field<NonNullGraphType<StringGraphType>, string>("nameUpper")
            .Resolve(_ => _.Source.Name.ToUpper());

        // Enum projection - demonstrates projecting scalar enum types
        Field<NonNullGraphType<StringGraphType>, string>("statusDisplay")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, string, EntityStatus>(
                projection: _ => _.Status,
                resolve: _ => _.Projection switch
                {
                    EntityStatus.Active => "Currently Active",
                    EntityStatus.Inactive => "Currently Inactive",
                    EntityStatus.Pending => "Pending Activation",
                    _ => "Unknown"
                });

        // Navigation property access DOES use projection-based resolve
        Field<NonNullGraphType<StringGraphType>, string>("parentName")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, string, FieldBuilderProjectionParentEntity?>(
                projection: _ => _.Parent,
                resolve: _ => _.Projection?.Name ?? "No Parent");

        // WithProjection sets metadata only (no resolver wrapping) — ensures scalar fields
        // are included in the parent query's SELECT projection even when not explicitly queried
        Field<NonNullGraphType<StringGraphType>, string>("statusViaWithProjection")
            .WithProjection((Expression<Func<FieldBuilderProjectionEntity, EntityStatus>>)(_ => _.Status))
            .Resolve(_ => _.Source.Status switch
            {
                EntityStatus.Active => "Active via WithProjection",
                EntityStatus.Inactive => "Inactive via WithProjection",
                EntityStatus.Pending => "Pending via WithProjection",
                _ => "Unknown"
            });

        AutoMap(exclusions: [nameof(FieldBuilderProjectionEntity.Parent)]);
    }
}
