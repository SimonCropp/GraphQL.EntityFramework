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

                var query = dataContext.Set<TestEntity>();

                return query
                    .ApplyGraphQlArguments(context)
                    .ToListAsync();
            });
    }
}