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

        // Detect two patterns:
        // 1. filters.For<T>().Add(filter: (_, _, _, e) => ...) - 4-param filter without projection (GQLEF005)
        // 2. filters.For<T>().Add(projection: _ => _, filter: ...) - explicit identity projection (GQLEF004/GQLEF006)

        if (!IsFilterBuilderAdd(invocation, context.SemanticModel, out var projectionLambda, out var filterLambda, out var entityType))
        {
            return;
        }

        if (filterLambda == null)
        {
            return;
        }

        // Check if there's an explicit projection parameter
        if (projectionLambda != null)
        {
            // Pattern 2: filters.For<T>().Add(projection: ..., filter: ...)
            if (!IsIdentityProjection(projectionLambda))
            {
                return;
            }

            // Identity projection detected
            // First check for abstract navigation access (GQLEF007)
            var abstractNav = FindAbstractNavigationAccess(filterLambda, context.SemanticModel);
            if (abstractNav != null)
            {
                // Error: Identity projection with abstract navigation access
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GQLEF007,
                    invocation.GetLocation(),
                    entityType ?? "Entity",
                    abstractNav);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            // Then check for non-key property access (GQLEF006)
            var nonKeyProperty = FindNonKeyPropertyAccess(filterLambda, context.SemanticModel);
            if (nonKeyProperty != null)
            {
                // Error: Identity projection with non-key access
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GQLEF006,
                    invocation.GetLocation(),
                    nonKeyProperty);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                // Info: Suggest simplified API
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GQLEF004,
                    invocation.GetLocation(),
                    entityType);
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // Pattern 1: filters.For<T>().Add(filter: (_, _, _, e) => ...) - no explicit projection
        // Check if this is a 4-parameter filter (identity projection without explicit projection parameter)
        var is4ParamFilter = Is4ParameterFilter(filterLambda);

        if (is4ParamFilter)
        {
            // First check for abstract navigation access (GQLEF007)
            var abstractNav = FindAbstractNavigationAccess(filterLambda, context.SemanticModel);
            if (abstractNav != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GQLEF007,
                    invocation.GetLocation(),
                    entityType ?? "Entity",
                    abstractNav);
                context.ReportDiagnostic(diagnostic);
                return;
            }

            // Then validate that filter only accesses PK/FK properties
            var nonKeyProperty = FindNonKeyPropertyAccess(filterLambda, context.SemanticModel);
            if (nonKeyProperty != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GQLEF005,
                    invocation.GetLocation(),
                    nonKeyProperty);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    static bool Is4ParameterFilter(LambdaExpressionSyntax lambda)
    {
        // Check if the lambda has 4 parameters (userContext, dbContext, userPrincipal, entity)
        if (lambda is SimpleLambdaExpressionSyntax)
        {
            return false; // Single parameter lambda
        }

        if (lambda is ParenthesizedLambdaExpressionSyntax parenthesized)
        {
            return parenthesized.ParameterList.Parameters.Count == 4;
        }

        return false;
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

        // Check method name is Add
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        if (memberAccess.Name.Identifier.Text != "Add")
        {
            return false;
        }

        // Get the symbol info
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        // Check if it's the FilterBuilder API: filters.For<T>().Add(projection: ..., filter: ...)
        // The containing type should be FilterBuilder<TDbContext, TEntity>
        var containingType = methodSymbol.ContainingType;
        if (containingType is not { Name: "FilterBuilder" } ||
            containingType.ContainingNamespace?.ToString() != "GraphQL.EntityFramework")
        {
            return false;
        }

        // Extract entity type from FilterBuilder<TDbContext, TEntity>
        if (containingType.TypeArguments.Length >= 2)
        {
            entityType = containingType.TypeArguments[1].Name;
        }

        // Find the projection and filter parameters
        var projectionParameter = methodSymbol.Parameters.FirstOrDefault(p => p.Name == "projection");
        var filterParameter = methodSymbol.Parameters.FirstOrDefault(p => p.Name == "filter");

        if (projectionParameter == null && filterParameter == null)
        {
            return false;
        }

        // Extract arguments
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

        // Try positional arguments if named not found
        if (projectionLambda == null && projectionParameter != null)
        {
            var parameterIndex = GetParameterIndex(methodSymbol.Parameters, projectionParameter);
            if (parameterIndex >= 0 && parameterIndex < invocation.ArgumentList.Arguments.Count)
            {
                var arg = invocation.ArgumentList.Arguments[parameterIndex];
                if (arg.Expression is LambdaExpressionSyntax lambda)
                {
                    projectionLambda = lambda;
                }
            }
        }

        if (filterLambda == null && filterParameter != null)
        {
            var parameterIndex = GetParameterIndex(methodSymbol.Parameters, filterParameter);
            if (parameterIndex >= 0 && parameterIndex < invocation.ArgumentList.Arguments.Count)
            {
                var arg = invocation.ArgumentList.Arguments[parameterIndex];
                if (arg.Expression is LambdaExpressionSyntax lambda)
                {
                    filterLambda = lambda;
                }
            }
        }

        return projectionLambda != null || filterLambda != null;
    }

    static bool IsIdentityProjection(LambdaExpressionSyntax? lambda)
    {
        // Check if it's an identity projection (x => x or _ => _)
        // Lambda body should be a simple parameter reference that matches the lambda parameter
        if (lambda?.Body is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        // Get the parameter name (e.g., "x", "_", "item")
        var parameterName = lambda is SimpleLambdaExpressionSyntax simpleLambda
            ? simpleLambda.Parameter.Identifier.Text
            : lambda is ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesizedLambda
                ? parenthesizedLambda.ParameterList.Parameters[0].Identifier.Text
                : null;

        // Check if the body references the same parameter
        return parameterName != null && identifier.Identifier.Text == parameterName;
    }

    static string? FindNonKeyPropertyAccess(LambdaExpressionSyntax lambda, SemanticModel semanticModel)
    {
        var body = lambda.Body;

        // Get the filter lambda parameter name (last parameter in the filter signature)
        string? filterParameterName = null;
        if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
        {
            filterParameterName = simpleLambda.Parameter.Identifier.Text;
        }
        else if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            // Filter signature: (userContext, data, userPrincipal, entity) => ...
            // The last parameter is the entity
            var parameterCount = parenthesizedLambda.ParameterList.Parameters.Count;
            if (parameterCount > 0)
            {
                filterParameterName = parenthesizedLambda.ParameterList.Parameters[parameterCount - 1].Identifier.Text;
            }
        }

        if (filterParameterName == null)
        {
            return null;
        }

        // Find all member access expressions in the lambda
        var memberAccesses = body.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>();

        foreach (var memberAccess in memberAccesses)
        {
            // Check if this is accessing the filter parameter (e.g., e.Property, entity.Id)
            if (!IsFilterParameterAccess(memberAccess, filterParameterName, out var propertyAccess))
            {
                continue;
            }

            if (propertyAccess == null)
            {
                continue;
            }

            // Get the symbol for the property being accessed
            var symbolInfo = semanticModel.GetSymbolInfo(propertyAccess);
            if (symbolInfo.Symbol is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            // Check if this property is NOT a key property
            // Check primary key first (cheaper), then foreign key
            var isPrimaryKey = IsPrimaryKeyProperty(propertySymbol);
            if (!isPrimaryKey && !IsForeignKeyProperty(propertySymbol))
            {
                return propertySymbol.Name;
            }
        }

        return null;
    }

    static bool IsFilterParameterAccess(
        MemberAccessExpressionSyntax memberAccess,
        string filterParameterName,
        out MemberAccessExpressionSyntax? propertyAccess)
    {
        propertyAccess = null;

        // Check if the expression directly references the filter parameter
        // e.g., e.Property or entity.Id
        if (memberAccess.Expression is IdentifierNameSyntax identifier &&
            identifier.Identifier.Text == filterParameterName)
        {
            propertyAccess = memberAccess;
            return true;
        }

        // Check for nested access (e.g., e.Parent.Id)
        // We'll detect the first level property access
        if (memberAccess.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax nestedIdentifier } nestedAccess &&
            nestedIdentifier.Identifier.Text == filterParameterName)
        {
            propertyAccess = nestedAccess;
            return true;
        }

        return false;
    }

    static bool IsPrimaryKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;

        // Simple "Id" is always a primary key
        if (name == "Id")
        {
            return true;
        }

        // EntityId, CompanyId (where Entity/Company is the class name) are primary keys
        var containingType = propertySymbol.ContainingType;
        if (containingType != null && name.EndsWith("Id") && name.Length > 2)
        {
            // Check if the property name matches the type name + "Id"
            var typeName = containingType.Name;

            if (name == $"{typeName}Id")
            {
                return true;
            }

            // Check if removing common suffixes from typeName + "Id" equals name
            if (TryMatchWithoutSuffix(typeName, name, "Entity") ||
                TryMatchWithoutSuffix(typeName, name, "Model") ||
                TryMatchWithoutSuffix(typeName, name, "Dto"))
            {
                return true;
            }
        }

        return false;
    }

    static bool TryMatchWithoutSuffix(string typeName, string propertyName, string suffix)
    {
        if (!typeName.EndsWith(suffix) || typeName.Length <= suffix.Length)
        {
            return false;
        }

        var baseLength = typeName.Length - suffix.Length;
        // Check if propertyName is baseTypeName + "Id"
        return propertyName.Length == baseLength + 2 &&
               typeName.AsSpan(0, baseLength).SequenceEqual(propertyName.AsSpan(0, baseLength)) &&
               propertyName.EndsWith("Id");
    }

    static bool IsForeignKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;
        var type = propertySymbol.Type;

        // Foreign keys are typically nullable or non-nullable integer/Guid types ending with "Id"
        if (!name.EndsWith("Id") || name == "Id")
        {
            return false;
        }

        // Check if the type is a scalar type suitable for FK (int, long, Guid, etc.)
        // Unwrap nullable
        var underlyingType = type;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            underlyingType = namedType.TypeArguments[0];
        }

        // Foreign keys are typically int, long, Guid
        switch (underlyingType.SpecialType)
        {
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Int16:
                return true;
        }

        // Check for System.Guid using symbol comparison instead of string
        if (underlyingType is INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol is { SpecialType: SpecialType.None, Name: "Guid" } &&
                   namedTypeSymbol.ContainingNamespace?.ToDisplayString() == "System";
        }

        return false;
    }

    static int GetParameterIndex(ImmutableArray<IParameterSymbol> parameters, IParameterSymbol parameter)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            if (SymbolEqualityComparer.Default.Equals(parameters[i], parameter))
            {
                return i;
            }
        }
        return -1;
    }

    static string? FindAbstractNavigationAccess(
        LambdaExpressionSyntax lambda,
        SemanticModel semanticModel)
    {
        var body = lambda.Body;

        // Get the filter lambda parameter name (last parameter in the filter signature)
        string? filterParameterName = null;
        if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
        {
            filterParameterName = simpleLambda.Parameter.Identifier.Text;
        }
        else if (lambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            var parameterCount = parenthesizedLambda.ParameterList.Parameters.Count;
            if (parameterCount > 0)
            {
                filterParameterName = parenthesizedLambda.ParameterList.Parameters[parameterCount - 1].Identifier.Text;
            }
        }

        if (filterParameterName == null)
        {
            return null;
        }

        // Find all member access expressions in the lambda
        var memberAccesses = body.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>();

        foreach (var memberAccess in memberAccesses)
        {
            // Check for navigation property access pattern: e.Parent.Property
            // We want to detect when `e.Parent` is a navigation to an abstract type
            if (memberAccess.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } nestedAccess &&
                identifier.Identifier.Text == filterParameterName)
            {
                // This is e.Parent.Property - check if Parent is abstract
                if (semanticModel.GetSymbolInfo(nestedAccess).Symbol is IPropertySymbol { Type.IsAbstract: true } navigationSymbol)
                {
                    return navigationSymbol.Name;
                }
            }
            // Also check direct navigation access: e.Parent (without further property access)
            else if (memberAccess.Expression is IdentifierNameSyntax directIdentifier &&
                     directIdentifier.Identifier.Text == filterParameterName)
            {
                if (semanticModel.GetSymbolInfo(memberAccess).Symbol is IPropertySymbol propertySymbol)
                {
                    var propType = propertySymbol.Type;
                    // Check if this is a reference type (not a primitive) and is abstract
                    // This indicates it's likely a navigation property to an abstract entity
                    if (propType is { IsAbstract: true, TypeKind: TypeKind.Class })
                    {
                        // But make sure it's not a primitive key property or known scalar
                        if (!IsPrimaryKeyProperty(propertySymbol) && !IsForeignKeyProperty(propertySymbol))
                        {
                            return propertySymbol.Name;
                        }
                    }
                }
            }
        }

        return null;
    }
}
