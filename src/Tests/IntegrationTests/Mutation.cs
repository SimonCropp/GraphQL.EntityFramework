using GraphQL.EntityFramework;

public class Mutation :
    QueryGraphType<IntegrationDbContext>
{
    public Mutation(IEfGraphQLService<IntegrationDbContext> efGraphQlService) :
        base(efGraphQlService)
    {
        AddSingleField(
            name: "parentEntityMutation",
            resolve: context => context.DbContext.ParentEntities,
            mutate: (context, entity) =>
            {
                entity.Property = "Foo";
                return context.DbContext.SaveChangesAsync();
            });
    }
}