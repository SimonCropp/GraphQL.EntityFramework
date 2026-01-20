public class FieldBuilderProjectionGraphType :
    EfObjectGraphType<IntegrationDbContext, FieldBuilderProjectionEntity>
{
    public FieldBuilderProjectionGraphType(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        // Test value type projection - int
        Field<NonNullGraphType<IntGraphType>, int>("ageDoubled")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, int, int>(
                projection: _ => _.Age,
                resolve: _ => _.Projection * 2);

        // Test value type projection - bool
        Field<NonNullGraphType<BooleanGraphType>, bool>("isAdult")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, bool, int>(
                projection: _ => _.Age,
                resolve: _ => _.Projection >= 18);

        // Test value type projection - DateTime
        Field<NonNullGraphType<DateTimeGraphType>, DateTime>("createdYear")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, DateTime, DateTime>(
                projection: _ => _.CreatedAt,
                resolve: _ => new(_.Projection.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        // Test value type projection - decimal
        Field<NonNullGraphType<DecimalGraphType>, decimal>("salaryWithBonus")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, decimal, decimal>(
                projection: _ => _.Salary,
                resolve: _ => _.Projection * 1.1m);

        // Test value type projection - double
        Field<NonNullGraphType<FloatGraphType>, double>("scoreNormalized")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, double, double>(
                projection: _ => _.Score,
                resolve: _ => _.Projection / 100.0);

        // Test value type projection - long
        Field<NonNullGraphType<LongGraphType>, long>("viewCountDoubled")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, long, long>(
                projection: _ => _.ViewCount,
                resolve: _ => _.Projection * 2);

        // Test async value type projection - bool
        Field<NonNullGraphType<BooleanGraphType>, bool>("hasParentAsync")
            .ResolveAsync<IntegrationDbContext, FieldBuilderProjectionEntity, bool, Guid>(
                projection: _ => _.Id,
                resolve: async ctx =>
                {
                    await Task.Delay(1); // Simulate async operation
                    var dbContext = ctx.DbContext;
                    return await dbContext.Set<FieldBuilderProjectionParentEntity>()
                        .AnyAsync(_ => _.Children.Any(c => c.Id == ctx.Projection));
                });

        // Test async value type projection - int
        Field<NonNullGraphType<IntGraphType>, int>("siblingCountAsync")
            .ResolveAsync<IntegrationDbContext, FieldBuilderProjectionEntity, int, Guid>(
                projection: _ => _.Id,
                resolve: async ctx =>
                {
                    var dbContext = ctx.DbContext;
                    var parent = await dbContext.Set<FieldBuilderProjectionParentEntity>()
                        .FirstOrDefaultAsync(_ => _.Children.Any(c => c.Id == ctx.Projection));
                    return parent?.Children.Count ?? 0;
                });

        // Test reference type projection - string
        Field<NonNullGraphType<StringGraphType>, string>("nameUpper")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, string, string>(
                projection: _ => _.Name,
                resolve: _ => _.Projection.ToUpper());

        // Test accessing navigation property with projection
        Field<NonNullGraphType<StringGraphType>, string>("parentName")
            .Resolve<IntegrationDbContext, FieldBuilderProjectionEntity, string, FieldBuilderProjectionParentEntity?>(
                projection: x => x.Parent,
                resolve: _ => _.Projection?.Name ?? "No Parent");

        AutoMap(exclusions: [nameof(FieldBuilderProjectionEntity.Parent)]);
    }
}
