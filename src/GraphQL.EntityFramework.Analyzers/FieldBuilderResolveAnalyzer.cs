namespace GraphQL.EntityFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FieldBuilderResolveAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        [DiagnosticDescriptors.GQLEF002, DiagnosticDescriptors.GQLEF003];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if this is a Resolve or ResolveAsync call
        if (!IsFieldBuilderResolveCall(invocation, context.SemanticModel, out var lambdaExpression))
        {
            return;
        }

        // Check if we're within an EfGraphType class
        if (!IsInEfGraphType(invocation, context.SemanticModel))
        {
            return;
        }

        // Check if the lambda already uses projection-based extension methods (has 4 type parameters)
        var isProjectionBased = IsProjectionBasedResolve(invocation, context.SemanticModel);

        if (isProjectionBased)
        {
            // For projection-based methods, check for identity projection
            var hasIdentity = HasIdentityProjection(invocation, context.SemanticModel);

            if (hasIdentity)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GQLEF003,
                    invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
            return;
        }

        // Analyze lambda for navigation property access
        if (lambdaExpression != null && AccessesNavigationProperties(lambdaExpression, context.SemanticModel))
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.GQLEF002,
                invocation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    static bool IsFieldBuilderResolveCall(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out LambdaExpressionSyntax? lambdaExpression)
    {
        lambdaExpression = null;

        // Check method name is Resolve, ResolveAsync, ResolveList, or ResolveListAsync
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        var methodName = memberAccess.Name.Identifier.Text;
        if (methodName != "Resolve" && methodName != "ResolveAsync" &&
            methodName != "ResolveList" && methodName != "ResolveListAsync")
        {
            return false;
        }

        // Get the symbol info
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        // Check if the containing type is FieldBuilder<,>
        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
        {
            return false;
        }

        var baseType = containingType;
        while (baseType != null)
        {
            if (baseType.Name == "FieldBuilder" &&
                baseType.ContainingNamespace?.ToString() == "GraphQL.Builders")
            {
                // Extract lambda from arguments
                if (invocation.ArgumentList.Arguments.Count > 0)
                {
                    var firstArg = invocation.ArgumentList.Arguments[0].Expression;
                    if (firstArg is LambdaExpressionSyntax lambda)
                    {
                        lambdaExpression = lambda;
                    }
                }

                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    static bool IsProjectionBasedResolve(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        // Projection-based extension methods have 4 type parameters:
        // TDbContext, TSource, TReturn, TProjection
        // AND have a parameter named "projection"
        return methodSymbol.TypeArguments.Length == 4 &&
               methodSymbol.Parameters.Any(_ => _.Name == "projection") &&
               methodSymbol.ContainingNamespace?.ToString() == "GraphQL.EntityFramework";
    }

    static bool IsInEfGraphType(SyntaxNode node, SemanticModel semanticModel)
    {
        var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration == null)
        {
            return false;
        }

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null)
        {
            return false;
        }

        // Check if class inherits from EfObjectGraphType, EfInterfaceGraphType, or QueryGraphType
        var baseType = classSymbol.BaseType;
        while (baseType != null)
        {
            var typeName = baseType.Name;
            if (typeName is "EfObjectGraphType" or "EfInterfaceGraphType" or "QueryGraphType" &&
                baseType.ContainingNamespace?.ToString() == "GraphQL.EntityFramework")
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    static bool AccessesNavigationProperties(LambdaExpressionSyntax lambda, SemanticModel semanticModel)
    {
        var body = lambda.Body;

        // Find all member access expressions in the lambda
        var memberAccesses = body.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>();

        foreach (var memberAccess in memberAccesses)
        {
            // Check if this is accessing context.Source.PropertyName
            if (!IsContextSourceAccess(memberAccess, out var propertyAccess) || propertyAccess == null)
            {
                continue;
            }

            // Get the symbol for the property being accessed
            var symbolInfo = semanticModel.GetSymbolInfo(propertyAccess);
            if (symbolInfo.Symbol is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            // Check if this property is safe to access (PK or FK only)
            // Only primary keys and foreign keys are guaranteed to be loaded by projection
            if (!IsSafeProperty(propertySymbol))
            {
                return true;
            }
        }

        return false;
    }

    static bool IsContextSourceAccess(MemberAccessExpressionSyntax memberAccess, out MemberAccessExpressionSyntax? propertyAccess)
    {
        propertyAccess = null;

        // Check if the expression is context.Source or further nested
        var current = memberAccess;
        MemberAccessExpressionSyntax? sourceAccess = null;

        // Walk up the chain to find context.Source
        while (true)
        {
            if (current.Expression is
                MemberAccessExpressionSyntax
                {
                    Name.Identifier.Text: "Source",
                    Expression: IdentifierNameSyntax
                    {
                        Identifier.Text: "context" or "ctx" or "_"
                    }
                })
            {
                sourceAccess = current;
                break;
            }

            if (current.Expression is IdentifierNameSyntax { Identifier.Text: "Source" })
            {
                // This might be a simplified lambda like: Source => Source.Property
                sourceAccess = current;
                break;
            }

            // Check if expression is itself a member access
            if (current.Expression is MemberAccessExpressionSyntax parentAccess)
            {
                current = parentAccess;
            }
            else
            {
                break;
            }
        }

        if (sourceAccess != null)
        {
            propertyAccess = sourceAccess;
            return true;
        }

        // Also check for direct access like: context.Source (without further property access)
        if (memberAccess.Name.Identifier.Text == "Source" &&
            memberAccess is
            {
                Expression: IdentifierNameSyntax
                {
                    Identifier.Text: "context" or "ctx" or "_"
                },
                Parent: MemberAccessExpressionSyntax parentMember
            })
            // This is just context.Source itself, check parent node
        {
            propertyAccess = parentMember;
            return true;
        }

        return false;
    }

    static bool IsSafeProperty(IPropertySymbol propertySymbol) =>
        // Only primary keys and foreign keys are safe to access
        // because they are always included in EF projections
        // Check if it's a primary key (Id, EntityId, etc.)
        IsPrimaryKeyProperty(propertySymbol) ||
        // Check if it's a foreign key (ParentId, UserId, etc.)
        // Everything else (navigation properties, scalar properties) is unsafe
        IsForeignKeyProperty(propertySymbol);

    static bool IsPrimaryKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;

        // Simple "Id" is always a primary key
        if (name == "Id")
        {
            return true;
        }

        // EntityId, CompanyId (where Entity/Company is the class name) are primary keys
        // But ParentId, UserId (where Parent/User is a navigation) are foreign keys
        // We can't perfectly distinguish without EF metadata, so we use a heuristic:
        // If it ends with "Id" and has uppercase before "Id", it MIGHT be PK
        // We'll check the containing type name
        var containingType = propertySymbol.ContainingType;
        if (containingType != null && name.EndsWith("Id") && name.Length > 2)
        {
            // Check if the property name matches the type name + "Id"
            // e.g., property "CompanyId" in class "Company" or "CompanyEntity"
            var typeName = containingType.Name;

            if (name == $"{typeName}Id")
            {
                return true;
            }

            // Check if removing common suffixes from typeName + "Id" equals name
            // e.g., "CompanyEntity" -> "CompanyId"
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

        // If we already determined it's a primary key, it's not a foreign key
        if (IsPrimaryKeyProperty(propertySymbol))
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

        var typeName = underlyingType.ToString();
        return typeName == "System.Guid";
    }

    static bool HasIdentityProjection(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        // Get the symbol info to find parameter positions
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        // Find the "projection" parameter
        var projectionParameter = methodSymbol.Parameters.FirstOrDefault(_ => _.Name == "projection");
        if (projectionParameter == null)
        {
            return false;
        }

        // Find the corresponding argument (by name or position)
        ArgumentSyntax? projectionArgument = null;

        // First, try to find by named argument
        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            if (arg.NameColon?.Name.Identifier.Text == "projection")
            {
                projectionArgument = arg;
                break;
            }
        }

        // If not found by name, try positional (for the case where all args are positional)
        if (projectionArgument == null)
        {
            var parameterIndex = Array.IndexOf(methodSymbol.Parameters.ToArray(), projectionParameter);
            if (parameterIndex >= 0 && parameterIndex < invocation.ArgumentList.Arguments.Count)
            {
                projectionArgument = invocation.ArgumentList.Arguments[parameterIndex];
            }
        }

        if (projectionArgument?.Expression is not LambdaExpressionSyntax lambda)
        {
            return false;
        }

        // Check if it's an identity projection (x => x or _ => _)
        // Lambda body should be a simple parameter reference that matches the lambda parameter
        if (lambda.Body is not IdentifierNameSyntax identifier)
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
}
