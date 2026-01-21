static class DiagnosticDescriptors
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
        title: "Projection to scalar types is not allowed in projection-based Resolve methods",
        messageFormat: "Projection to scalar type '{0}' is not allowed. Projection-based Resolve methods are for loading navigation properties, not scalar properties. Use regular Resolve() method instead.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Projection-based Resolve extension methods are designed for loading navigation properties (related entities), not for accessing scalar properties like primitives, strings, dates, or enums. Use the regular Resolve() method to access and transform scalar properties.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#projection-based-resolve");
}
