using GraphQL.Resolvers;

namespace GraphQL.Builders
{
    /// <summary>
    /// Contains parameters pertaining to the currently executing <see cref="IFieldResolver"/>, along
    /// with helper properties for resolving forward and backward pagination requests on a
    /// connection type.
    /// </summary>
    public interface IResolvePaginationContext : IResolveFieldContext
    {

        /// <summary>
        /// Row
        /// </summary>
        int? Row { get; }
        
        /// <summary>
        /// Page
        /// </summary>
        int? Page { get; }
    }

    /// <inheritdoc cref="IResolvePaginationContext"/>
    public interface IResolvePaginationContext<out T> : IResolveFieldContext<T>, IResolvePaginationContext
    {
    }
}