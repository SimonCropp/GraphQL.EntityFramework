using Microsoft.CodeAnalysis;

namespace GraphQL.EntityFramework.Analyzers;

public static class DiagnosticDescriptors
{
    public const string DiagnosticId = "GQLEF001";

    public static readonly DiagnosticDescriptor ProblematicContextSourceAccess = new(
        id: DiagnosticId,
        title: "Avoid accessing non-ID properties on context.Source",
        messageFormat: "Property '{0}' accessed on context.Source may not be loaded. Consider using AddProjectedField/AddProjectedNavigationField instead, or access the foreign key '{1}' if available.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Entity Framework projections only guarantee primary keys and foreign keys are loaded. Accessing other properties on context.Source may result in null values.",
        helpLinkUri: "https://github.com/SimonCropp/GraphQL.EntityFramework#projections");
}
