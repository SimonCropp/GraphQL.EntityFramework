
namespace GraphQL.EntityFramework
{
    public delegate bool Filter<in TReturn>(object userContext, TReturn input);
}