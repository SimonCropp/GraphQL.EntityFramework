using GraphQL.EntityFramework;
using GraphQL.Types;

public class Query : ObjectGraphType
{
    public Query(EfGraphQLService efGraphQlService)
    {
        this.AddQueryField<TestEntityGraph, TestEntity>(
            name: "testEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.TestEntities;
            },
            efGraphQlService: efGraphQlService);

        this.AddQueryConnectionField<TestEntityGraph, TestEntity>(
            name: "testEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.TestEntities;
            },
            includeName: "TestEntities",
            efGraphQlService: efGraphQlService);
    }
}