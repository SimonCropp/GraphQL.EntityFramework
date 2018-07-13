using EfCoreGraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

public class Query : ObjectGraphType
{
    public Query(ResolveDataContext<MyDataContext> resolveDataContext)
    {
        Field<ListGraphType<TestEntityType>>(
            "testEntities",
            arguments: ArgumentAppender.DefaultArguments,
            resolve: context =>
            {
                var dataContext = resolveDataContext(context);
                return dataContext.TestEntities
                    .ApplyGraphQlArguments(context)
                    .ToListAsync();
            });
    }
}