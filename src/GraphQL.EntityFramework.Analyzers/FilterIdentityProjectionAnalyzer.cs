namespace GraphQL.EntityFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FilterIdentityProjectionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [
            DiagnosticDescriptors.GQLEF004,
            DiagnosticDescriptors.GQLEF005,
            DiagnosticDescriptors.GQLEF006,
            DiagnosticDescriptors.GQLEF007
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsFilterBuilderAdd(invocation, context.SemanticModel, out var projectionLambda, out var filterLambda, out var entityType))
        {
            return;
        }

        if (filterLambda == null)
        {
            return;
        }

        // Determine if this is an identity projection scenario:
        // 1. Explicit identity projection: filters.For<T>().Add(projection: _ => _, filter: ...)
        // 2. Implicit (4-param filter): filters.For<T>().Add(filter: (_, _, _, e) => ...)
        bool isExplicitIdentity;
        if (projectionLambda != null)
        {
            if (!IsIdentityProjection(projectionLambda))
            {
                return;
            }

            isExplicitIdentity = true;
        }
        else if (filterLambda is ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 4 })
        {
            isExplicitIdentity = false;
        }
        else
        {
            return;
        }

        // Both paths share the same analysis: abstract nav check, then non-key check
        var abstractNav = FindAbstractNavigationAccess(filterLambda, context.SemanticModel);
        if (abstractNav != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.GQLEF007,
                invocation.GetLocation(),
                entityType ?? "Entity",
                abstractNav));
            return;
        }

        var nonKeyProperty = FindNonKeyPropertyAccess(filterLambda, context.SemanticModel);
        if (nonKeyProperty != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                isExplicitIdentity ? DiagnosticDescriptors.GQLEF006 : DiagnosticDescriptors.GQLEF005,
                invocation.GetLocation(),
                nonKeyProperty));
            return;
        }

        // Only suggest simplified API for explicit identity projections that passed all checks
        if (isExplicitIdentity && !string.IsNullOrEmpty(entityType))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.GQLEF004,
                invocation.GetLocation(),
                entityType));
        }
    }

    static bool IsFilterBuilderAdd(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out LambdaExpressionSyntax? projectionLambda,
        out LambdaExpressionSyntax? filterLambda,
        out string? entityType)
    {
        projectionLambda = null;
        filterLambda = null;
        entityType = null;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Name.Identifier.Text != "Add")
        {
            return false;
        }

        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        var containingType = methodSymbol.ContainingType;
        if (containingType is not { Name: "FilterBuilder" } ||
            containingType.ContainingNamespace?.ToString() != "GraphQL.EntityFramework")
        {
            return false;
        }

        if (containingType.TypeArguments.Length >= 2)
        {
            entityType = containingType.TypeArguments[1].Name;
        }

        // Extract arguments (try named first, then positional)
        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            if (arg.NameColon?.Name.Identifier.Text == "projection" &&
                arg.Expression is LambdaExpressionSyntax projLambda)
            {
                projectionLambda = projLambda;
            }
            else if (arg.NameColon?.Name.Identifier.Text == "filter" &&
                     arg.Expression is LambdaExpressionSyntax filtLambda)
            {
                filterLambda = filtLambda;
            }
        }

        // Fall back to positional arguments
        foreach (var param in methodSymbol.Parameters)
        {
            if (param.Ordinal >= invocation.ArgumentList.Arguments.Count)
            {
                continue;
            }

            var arg = invocation.ArgumentList.Arguments[param.Ordinal];
            if (arg.NameColon != null || arg.Expression is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            if (param.Name == "projection" && projectionLambda == null)
            {
                projectionLambda = lambda;
            }
            else if (param.Name == "filter" && filterLambda == null)
            {
                filterLambda = lambda;
            }
        }

        return projectionLambda != null || filterLambda != null;
    }

    static bool IsIdentityProjection(LambdaExpressionSyntax lambda)
    {
        if (lambda.Body is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        var parameterName = lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesized =>
                parenthesized.ParameterList.Parameters[0].Identifier.Text,
            _ => null
        };

        return parameterName != null && identifier.Identifier.Text == parameterName;
    }

    static string? GetLastParameterName(LambdaExpressionSyntax lambda) =>
        lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: > 0 } parenthesized =>
                parenthesized.ParameterList.Parameters[parenthesized.ParameterList.Parameters.Count - 1].Identifier.Text,
            _ => null
        };

    static string? FindNonKeyPropertyAccess(LambdaExpressionSyntax lambda, SemanticModel semanticModel)
    {
        var paramName = GetLastParameterName(lambda);
        if (paramName == null)
        {
            return null;
        }

        foreach (var memberAccess in lambda.Body.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>())
        {
            // Find the property access rooted on the filter parameter
            MemberAccessExpressionSyntax? propertyAccess = null;

            if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == paramName)
            {
                // Direct: e.Property
                propertyAccess = memberAccess;
            }
            else if (memberAccess.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax nestedId } nestedAccess &&
                     nestedId.Identifier.Text == paramName)
            {
                // Nested: e.Parent.Property - check the first-level property
                propertyAccess = nestedAccess;
            }

            if (propertyAccess == null)
            {
                continue;
            }

            if (semanticModel.GetSymbolInfo(propertyAccess).Symbol is IPropertySymbol propertySymbol &&
                !IsKeyProperty(propertySymbol))
            {
                return propertySymbol.Name;
            }
        }

        return null;
    }

    static string? FindAbstractNavigationAccess(LambdaExpressionSyntax lambda, SemanticModel semanticModel)
    {
        var paramName = GetLastParameterName(lambda);
        if (paramName == null)
        {
            return null;
        }

        foreach (var memberAccess in lambda.Body.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>())
        {
            // Check for: e.Parent.Property where Parent is abstract
            if (memberAccess.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } nestedAccess &&
                identifier.Identifier.Text == paramName)
            {
                if (semanticModel.GetSymbolInfo(nestedAccess).Symbol is IPropertySymbol { Type.IsAbstract: true } navigationSymbol)
                {
                    return navigationSymbol.Name;
                }
            }
            // Check for: e.Parent where Parent is abstract class
            else if (memberAccess.Expression is IdentifierNameSyntax directIdentifier &&
                     directIdentifier.Identifier.Text == paramName &&
                     semanticModel.GetSymbolInfo(memberAccess).Symbol is IPropertySymbol { Type: { IsAbstract: true, TypeKind: TypeKind.Class } } propSymbol &&
                     !IsKeyProperty(propSymbol))
            {
                return propSymbol.Name;
            }
        }

        return null;
    }

    static bool IsKeyProperty(IPropertySymbol propertySymbol) =>
        IsPrimaryKeyProperty(propertySymbol) || IsForeignKeyProperty(propertySymbol);

    static bool IsPrimaryKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;

        if (name == "Id")
        {
            return true;
        }

        var containingType = propertySymbol.ContainingType;
        if (containingType == null || !name.EndsWith("Id") || name.Length <= 2)
        {
            return false;
        }

        var typeName = containingType.Name;

        if (name == $"{typeName}Id")
        {
            return true;
        }

        return TryMatchWithoutSuffix(typeName, name, "Entity") ||
               TryMatchWithoutSuffix(typeName, name, "Model") ||
               TryMatchWithoutSuffix(typeName, name, "Dto");
    }

    static bool TryMatchWithoutSuffix(string typeName, string propertyName, string suffix)
    {
        if (!typeName.EndsWith(suffix) || typeName.Length <= suffix.Length)
        {
            return false;
        }

        var baseLength = typeName.Length - suffix.Length;
        return propertyName.Length == baseLength + 2 &&
               typeName.AsSpan(0, baseLength).SequenceEqual(propertyName.AsSpan(0, baseLength)) &&
               propertyName.EndsWith("Id");
    }

    static bool IsForeignKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;
        if (!name.EndsWith("Id") || name == "Id")
        {
            return false;
        }

        // Unwrap nullable
        var type = propertySymbol.Type;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        return type.SpecialType is SpecialType.System_Int32 or SpecialType.System_Int64 or SpecialType.System_Int16 ||
               type is INamedTypeSymbol { SpecialType: SpecialType.None, Name: "Guid", ContainingNamespace.Name: "System" };
    }
}
