using GraphQL.EntityFramework;
using GraphQL.Types;

public class MultiContextQuery :
    ObjectGraphType
{
    public MultiContextQuery(
        IEfGraphQLService<DbContext1> efGraphQlService1,
        IEfGraphQLService<DbContext2> efGraphQlService2)
    {
        efGraphQlService1.AddSingleField(
            graph: this,
            name: "entity1",
            resolve: context =>
            {
                var userContext = (UserContext) context.UserContext;
                return userContext.DbContext1.Entities;
            });
        efGraphQlService2.AddSingleField(
            graph: this,
            name: "entity2",
            resolve: context =>
            {
                var userContext = (UserContext) context.UserContext;
                return userContext.DbContext2.Entities;
            });
    }
}