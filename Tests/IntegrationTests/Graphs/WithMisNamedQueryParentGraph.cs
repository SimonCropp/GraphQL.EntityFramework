using System.Linq;
using GraphQL.EntityFramework;

public class WithMisNamedQueryParentGraph : EfObjectGraphType<WithMisNamedQueryParentEntity>
{
    public WithMisNamedQueryParentGraph(IEfGraphQLService graphQlService) : base(graphQlService)
    {
        Field(x => x.Id);
        AddQueryField<WithMisNamedQueryChildGraph, WithMisNamedQueryChildEntity>(
            name: "misNamedChildren",
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                var parentId = context.Source.Id;
                return dataContext.WithMisNamedQueryChildEntities.Where(x=>x.ParentId == parentId);
            });
    }
}