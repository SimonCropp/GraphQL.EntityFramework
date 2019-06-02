namespace GraphQL.EntityFramework
{
    public delegate TDbContext DbContextFromUserContext<out TDbContext>(object userContext);
}