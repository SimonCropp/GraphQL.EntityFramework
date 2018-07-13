using EfCoreGraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

public class Query : ObjectGraphType
{
    public Query()
    {
        Field<ListGraphType<TestEntityGraph>>(
            "testEntities",
            arguments: ArgumentAppender.DefaultArguments,
            resolve: context =>
            {
                var dataContext = (MyDataContext)context.UserContext;
                return dataContext.TestEntities
                    .ApplyGraphQlArguments(context)
                    .ToListAsync();
            });
    }
}