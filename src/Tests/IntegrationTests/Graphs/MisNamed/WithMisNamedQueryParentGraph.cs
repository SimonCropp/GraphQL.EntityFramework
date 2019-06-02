using System.Linq;
using GraphQL.EntityFramework;

public class WithMisNamedQueryParentGraph :
    EfObjectGraphType<IntegrationDbContext, WithMisNamedQueryParentEntity>
{
    public WithMisNamedQueryParentGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        Field(x => x.Id);
        AddQueryField(
            name: "misNamedChildren",
            resolve: context =>
            {
                var parentId = context.Source.Id;
                return context.DbContext.WithMisNamedQueryChildEntities
                    .Where(x => x.ParentId == parentId);
            });
    }
}