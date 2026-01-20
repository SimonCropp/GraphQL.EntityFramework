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
}
