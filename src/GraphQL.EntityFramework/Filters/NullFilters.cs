using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphQL.EntityFramework
{
    class NullFilters :
        Filters
    {
        public static NullFilters Instance = new NullFilters();
        internal override Task<IEnumerable<TEntity>> ApplyFilter<TEntity>(IEnumerable<TEntity> result, object userContext)
        {
            return Task.FromResult(result);
        }

        // Nullability of reference types in type of parameter doesn't match overridden member.
        #pragma warning disable CS8610
        internal override Task<bool> ShouldInclude<TEntity>(object userContext, TEntity item)
        {
            return Task.FromResult(true);
        }
    }
}