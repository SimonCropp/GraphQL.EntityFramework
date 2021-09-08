namespace GraphQL.EntityFramework.GraphApi
{
    public interface IFilterResolver
    {
        public Filters? ResolveFilters(object userContext);
    }
}