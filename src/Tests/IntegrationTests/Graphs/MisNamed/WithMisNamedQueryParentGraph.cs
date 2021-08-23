using System.Linq;
using GraphQL;
using GraphQL.EntityFramework;

[GraphQLMetadata("WithMisNamedQueryParent")]
public class WithMisNamedQueryParentGraph :
    EfObjectGraphType<IntegrationDbContext, WithMisNamedQueryParentEntity>
{
    public WithMisNamedQueryParentGraph(IEfGraphQLService<IntegrationDbContext> graphQlService) :
        base(graphQlService)
    {
        AddQueryField(
            name: "misNamedChildren",
            resolve: context =>
            {
                var parentId = context.Source!.Id;
                return context.DbContext.WithMisNamedQueryChildEntities
                    .Where(x => x.ParentId == parentId);
            });
        AutoMap();
    }
}