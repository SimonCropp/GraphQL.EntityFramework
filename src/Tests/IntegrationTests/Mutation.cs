public class Mutation :
    QueryGraphType<IntegrationDbContext>
{
    public Mutation(IEfGraphQLService<IntegrationDbContext> efGraphQlService) :
        base(efGraphQlService)
    {
        AddSingleField(
            name: "parentEntityMutation",
            resolve: _ => _.DbContext.ParentEntities,
            mutate: (context, entity) =>
            {
                entity.Property = "Foo";
                return context.DbContext.SaveChangesAsync();
            });

        // A resolver of its own, reading back the entity it changed with the service's projection
        Field<ParentGraphType>("changeParentAndReadBack")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .ResolveAsync(async context =>
            {
                var id = context.GetArgument<Guid>("id");
                var data = ResolveDbContext(context);
                var entity = await data.ParentEntities.SingleAsync(_ => _.Id == id);
                entity.Property = "Foo";
                await data.SaveChangesAsync();

                var query = data.ParentEntities
                    .AsSplitQuery()
                    .Where(_ => _.Id == id);
                return await GraphQlService.ApplyProjection(context, query).SingleAsync();
            });
    }
}
