namespace GraphQL.EntityFramework;

public partial class EfGraphQLService<TDbContext> :
    IEfGraphQLService<TDbContext>
    where TDbContext : DbContext
{
    ResolveFilters<TDbContext>? resolveFilters;
    bool disableTracking;
    bool includeSqlInExceptions;
    OrderByStyle orderByStyle;
    ResolveDbContext<TDbContext> resolveDbContext;
    IReadOnlyDictionary<Type, List<string>> keyNames;

    /// <param name="disableTracking">Use <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{TEntity}"/> for all <see cref="IQueryable{T}"/> operations.</param>
    /// <param name="includeSqlInExceptions">Include the generated sql in exception messages. Off by default, since those messages can reach clients and the sql carries table and column names and, depending on the provider, parameter values.</param>
    /// <param name="orderByStyle">The shape of the generated orderBy argument.</param>
    public EfGraphQLService(
        IModel model,
        ResolveDbContext<TDbContext> resolveDbContext,
        ResolveFilters<TDbContext>? resolveFilters = null,
        bool disableTracking = false,
        bool includeSqlInExceptions = false,
        OrderByStyle orderByStyle = OrderByStyle.Enum)
    {
        this.resolveFilters = resolveFilters;
        this.disableTracking = disableTracking;
        this.includeSqlInExceptions = includeSqlInExceptions;
        this.orderByStyle = orderByStyle;
        this.resolveDbContext = resolveDbContext;
        Model = model;

        keyNames = model.GetKeyNames();
        var foreignKeys = ForeignKeyExtractor.GetForeignKeyProperties(model);

        Navigations = NavigationReader.GetNavigationProperties(model);

        // A type with derived types is projected as a chain of type tests, most derived first,
        // so the derived types are listed base most first for wrapping
        var derivedTypes = model.GetEntityTypes()
            .Where(_ => _.GetDirectlyDerivedTypes().Any())
            .ToDictionary(
                _ => _.ClrType, IReadOnlyList<Type> (_) =>
                    _.GetDerivedTypes()
                    .OrderBy(derived => Depth(derived.ClrType))
                    .Select(derived => derived.ClrType)
                    .ToList());
        includeAppender = new(Navigations, keyNames, foreignKeys, derivedTypes);
    }

    static int Depth(Type type)
    {
        var depth = 0;
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            depth++;
        }

        return depth;
    }

    public IReadOnlyDictionary<Type, IReadOnlyDictionary<string, Navigation>> Navigations { get; }

    public IModel Model { get; }

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
            Filters = ResolveFilters(context),
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

    public Filters<TDbContext>? ResolveFilters(IResolveFieldContext context) =>
        resolveFilters?.Invoke(context.UserContext);

    /// <summary>
    /// The generated sql carries table and column names and, depending on the provider, the parameter
    /// values too. These messages surface as GraphQL errors, so the sql is opt in rather than default.
    /// </summary>
    string QueryText(IQueryable? query)
    {
        if (!includeSqlInExceptions)
        {
            return "(omitted, pass includeSqlInExceptions when registering to include the generated sql)";
        }

        if (query is null)
        {
            return "(none)";
        }

        return query.SafeToQueryString();
    }

    /// <summary>
    /// The generated sql for a not found exception, or null when it should not be disclosed.
    /// </summary>
    string? NotFoundQueryText(IQueryable? query)
    {
        if (!includeSqlInExceptions)
        {
            return null;
        }

        return query?.SafeToQueryString();
    }

    static string JoinKeys(IReadOnlyCollection<string>? names)
    {
        if (names == null)
        {
            return "";
        }

        return string.Join(", ", names);
    }
}
