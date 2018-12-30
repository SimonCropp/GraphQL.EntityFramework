
namespace GraphQL.EntityFramework
{
    public delegate bool Filter<in T>(object userContext, T input);
}