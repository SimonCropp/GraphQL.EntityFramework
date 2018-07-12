using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

public class Query : ObjectGraphType
{
    public Query()
    {
        Field<ListGraphType<ParentGraph>>(
            "parents",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Parents
                    .ToListAsync();
            });
    }
}