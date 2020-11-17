
namespace GraphQL.Builders
{
    /// <summary>
    /// A mutable implementation of <see cref="IResolvePaginationContext{T}"/>
    /// </summary>
    public class ResolvePaginationContext<T> : ResolveFieldContext<T>, IResolvePaginationContext<T>
    {
        private readonly int? _defaultRow;
        private readonly int? _defaultPage;

        /// <summary>
        /// Initializes an instance which mirrors the specified <see cref="IResolveFieldContext"/>
        /// with the specified properties and defaults
        /// </summary>
        /// <param name="context">The underlying <see cref="IResolveFieldContext"/> to mirror</param>
        /// <param name="defaultPage">Indicates if the default page only allows forward paging requests</param>
        /// <param name="defaultRow">Indicates the default row if not specified by the request</param>
        public ResolvePaginationContext(IResolveFieldContext context, int? defaultPage ,int? defaultRow)
            : base(context)
        {
            _defaultPage = defaultPage;
            _defaultRow = defaultRow;
        }



        /// <inheritdoc/>
        public int? Row => this.GetArgument<int?>("row") ?? _defaultRow;
        public int? Page =>  this.GetArgument<int?>("row") ??  _defaultPage;
    }
}