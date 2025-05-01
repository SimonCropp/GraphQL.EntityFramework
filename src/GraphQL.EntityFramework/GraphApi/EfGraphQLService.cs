namespace GraphQL.EntityFramework;

public partial class EfGraphQLService<TDbContext> :
    IEfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    ResolveFilters<TDbContext>? resolveFilters;
    bool disableTracking;
    ResolveDbContext<TDbContext> resolveDbContext;
    IReadOnlyDictionary<Type, List<string>> keyNames;

    /// <param name="disableTracking">Use <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> for all <see cref="IQueryable{T}"/> operations.</param>
    public EfGraphQLService(
        IModel model,
        ResolveDbContext<TDbContext> resolveDbContext,
        ResolveFilters<TDbContext>? resolveFilters = null,
        bool disableTracking = false)
    {
        this.resolveFilters = resolveFilters;
        this.disableTracking = disableTracking;
        this.resolveDbContext = resolveDbContext;

        keyNames = model.GetKeyNames();

        Navigations = NavigationReader.GetNavigationProperties(model);
        includeAppender = new(Navigations);
    }

    public IReadOnlyDictionary<Type, IReadOnlyList<Navigation>> Navigations { get; }

    IncludeAppender includeAppender;

    ResolveEfFieldContext<TDbContext, TSource> BuildContext<TSource>(
        IResolveFieldContext<TSource> context) =>
        new()
        {
            UserContext = context.UserContext,
            Arguments = context.Arguments,
            CancellationToken = context.CancellationToken,
            Document = context.Document,
            Errors = context.Errors,
            FieldAst = context.FieldAst,
            FieldDefinition = context.FieldDefinition,
            Metrics = context.Metrics,
            Operation = context.Operation,
            ParentType = context.ParentType,
            Path = context.Path,
            RootValue = context.RootValue,
            Schema = context.Schema,
            Source = context.Source,
            SubFields = context.SubFields,
            Variables = context.Variables,
            DbContext = ResolveDbContext(context),
            Filters = ResolveFilter(context),
            RequestServices = context.RequestServices,
            ResponsePath = context.ResponsePath,
            ArrayPool = context.ArrayPool,
            Parent = context.Parent,
            Directives = context.Directives,
            InputExtensions = context.InputExtensions,
            OutputExtensions = context.OutputExtensions,
            User = context.User
        };

    public TDbContext ResolveDbContext(IResolveFieldContext fieldContext)
    {
        var userContext = fieldContext.UserContext;
        var executionContext = fieldContext.ExecutionContext;
        var requestServices = executionContext.RequestServices ?? executionContext.ExecutionOptions.RequestServices;
        return resolveDbContext(userContext, requestServices);
    }

    Filters<TDbContext>? ResolveFilter<TSource>(IResolveFieldContext<TSource> context) =>
        resolveFilters?.Invoke(context.UserContext);

    public IQueryable<TItem> AddIncludes<TItem>(IQueryable<TItem> query, IResolveFieldContext context)
        where TItem : class =>
        includeAppender.AddIncludes(query, context);

    static string JoinKeys(IReadOnlyCollection<string>? names)
    {
        if (names == null)
        {
            return "";
        }

        return string.Join(", ", names);
    }
}