using System.Linq;
using GraphQL.EntityFramework;

public class WithMisNamedQueryParentGraph :
    EfObjectGraphType<MyDbContext, WithMisNamedQueryParentEntity>
{
    public WithMisNamedQueryParentGraph(IEfGraphQLService<MyDbContext> graphQlService) :
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