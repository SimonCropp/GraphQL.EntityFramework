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
                var dbContext = (MyDbContext)context.UserContext;
                var parentId = context.Source.Id;
                return dbContext.WithMisNamedQueryChildEntities
                    .Where(x=>x.ParentId == parentId);
            });
    }
}