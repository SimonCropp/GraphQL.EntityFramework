public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor GQLEF002 = new(
        id: "GQLEF002",
        title: "Use projection-based Resolve extension methods when accessing navigation properties",
        messageFormat: "Field().Resolve() or Field().ResolveAsync() may access navigation properties that aren't loaded. Use projection-based extension methods instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "When using Field().Resolve() or Field().ResolveAsync() inside EfObjectGraphType, EfInterfaceGraphType, or QueryGraphType classes, navigation properties on context.Source may not be loaded due to EF projection. Use the projection-based extension methods (Resolve<TDbContext, TSource, TReturn, TProjection>, ResolveAsync<TDbContext, TSource, TReturn, TProjection>, etc.) to ensure required data is loaded.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#projection-based-resolve");

    public static readonly DiagnosticDescriptor GQLEF003 = new(
        id: "GQLEF003",
        title: "Identity projection is not allowed in projection-based Resolve methods",
        messageFormat: "Identity projection '_ => _' defeats the purpose of projection-based Resolve. Use regular Resolve() method for PK/FK access, or specify navigation properties in projection.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Using '_ => _' as the projection parameter in projection-based Resolve extension methods is not allowed because it doesn't load any additional navigation properties. If you only need to access primary key or foreign key properties, use the regular Resolve() method instead. If you need to access navigation properties, specify them in the projection (e.g., 'x => x.Parent').",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#projection-based-resolve");

    public static readonly DiagnosticDescriptor GQLEF004 = new(
        id: "GQLEF004",
        title: "Use simplified filter API for identity projections",
        messageFormat: "Identity projection '_ => _' detected. Consider using simplified 4-parameter filter instead of explicit projection.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When using identity projection '_ => _' with a filter that only accesses primary key or foreign key properties, use the simplified 4-parameter filter syntax filters.For<TEntity>().Add(filter: (_, _, _, e) => ...) instead of specifying the projection explicitly.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#simplified-filter-api");

    public static readonly DiagnosticDescriptor GQLEF005 = new(
        id: "GQLEF005",
        title: "Filter with 4-parameter syntax must only access primary key or foreign key properties",
        messageFormat: "Filter accesses '{0}' which is not a primary key or foreign key. The 4-parameter filter uses identity projection, which only loads keys.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The 4-parameter filter syntax filters.For<TEntity>().Add(filter: (_, _, _, e) => ...) uses identity projection internally, which only guarantees that primary keys and foreign keys are loaded. Use filters.For<TEntity>().Add(projection: ..., filter: ...) to project required properties.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#simplified-filter-api");

    public static readonly DiagnosticDescriptor GQLEF006 = new(
        id: "GQLEF006",
        title: "Identity projection with filter that accesses non-key properties is invalid",
        messageFormat: "Filter accesses '{0}' which is not a primary key or foreign key, but projection is '_ => _'. Project the required properties instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "When using identity projection '_ => _', only primary keys and foreign keys are guaranteed to be loaded. Either project specific properties needed or use the simplified API if you only need key properties.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#identity-projection-filters");

    public static readonly DiagnosticDescriptor GQLEF007 = new(
        id: "GQLEF007",
        title: "Identity projection with abstract navigation access",
        messageFormat: "Filter for '{0}' uses identity projection but accesses abstract navigation '{1}'. This forces Include() to load all columns. Use explicit projection to load only required properties.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Abstract type navigations cannot use SQL projections and fall back to Include() which loads all columns. Use explicit projection to extract only required properties from abstract navigations.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#abstract-navigation-projections");
}
