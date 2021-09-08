namespace GraphQL.EntityFramework.GraphApi
{
    public interface IFilterResolver
    {
        public Filters.Filters? ResolveFilters(object userContext);
    }
}