#nullable enable

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GraphQL.EntityFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FieldBuilderResolveAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.GQLEF002);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
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
        if (IsProjectionBasedResolve(invocation, context.SemanticModel))
        {
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

    bool IsFieldBuilderResolveCall(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out LambdaExpressionSyntax? lambdaExpression)
    {
        lambdaExpression = null;

        // Check method name is Resolve, ResolveAsync, ResolveList, or ResolveListAsync
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null)
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

    bool IsProjectionBasedResolve(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        // Projection-based extension methods have 4 type parameters:
        // TDbContext, TSource, TReturn, TProjection
        return methodSymbol.TypeArguments.Length == 4 &&
               methodSymbol.ContainingNamespace?.ToString() == "GraphQL.EntityFramework";
    }

    bool IsInEfGraphType(SyntaxNode node, SemanticModel semanticModel)
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
            if ((typeName == "EfObjectGraphType" ||
                 typeName == "EfInterfaceGraphType" ||
                 typeName == "QueryGraphType") &&
                baseType.ContainingNamespace?.ToString() == "GraphQL.EntityFramework")
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    bool AccessesNavigationProperties(LambdaExpressionSyntax lambda, SemanticModel semanticModel)
    {
        var body = lambda.Body;
        if (body == null)
        {
            return false;
        }

        // Find all member access expressions in the lambda
        var memberAccesses = body.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>()
            .ToList();

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

            // Check if this property is a navigation property
            if (IsNavigationProperty(propertySymbol))
            {
                return true;
            }

            // Check if this is accessing a nested property (e.g., context.Source.Parent.Name)
            // This is also problematic because Parent might not be loaded
            if (IsNestedPropertyAccess(propertyAccess))
            {
                return true;
            }
        }

        return false;
    }

    bool IsContextSourceAccess(MemberAccessExpressionSyntax memberAccess, out MemberAccessExpressionSyntax? propertyAccess)
    {
        propertyAccess = null;

        // Check if the expression is context.Source or further nested
        var current = memberAccess;
        MemberAccessExpressionSyntax? sourceAccess = null;

        // Walk up the chain to find context.Source
        while (current != null)
        {
            if (current.Expression is MemberAccessExpressionSyntax innerAccess &&
                innerAccess.Name.Identifier.Text == "Source" &&
                innerAccess.Expression is IdentifierNameSyntax identifier &&
                (identifier.Identifier.Text == "context" || identifier.Identifier.Text == "ctx"))
            {
                sourceAccess = current;
                break;
            }

            if (current.Expression is IdentifierNameSyntax id &&
                id.Identifier.Text == "Source")
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
            memberAccess.Expression is IdentifierNameSyntax ctxId &&
            (ctxId.Identifier.Text == "context" || ctxId.Identifier.Text == "ctx"))
        {
            // This is just context.Source itself, check parent node
            if (memberAccess.Parent is MemberAccessExpressionSyntax parentMember)
            {
                propertyAccess = parentMember;
                return true;
            }
        }

        return false;
    }

    bool IsNavigationProperty(IPropertySymbol propertySymbol)
    {
        var propertyType = propertySymbol.Type;

        // Check if it's a primary key (Id, EntityId, etc.) - these are safe
        if (IsPrimaryKeyProperty(propertySymbol))
        {
            return false;
        }

        // Check if it's a foreign key (ParentId, UserId, etc.) - these are safe
        if (IsForeignKeyProperty(propertySymbol))
        {
            return false;
        }

        // Check if it's a scalar type - these are safe
        if (IsScalarType(propertyType))
        {
            return false;
        }

        // Check if it's a navigation property (entity type or collection)
        // Navigation properties are typically:
        // 1. Of a class type (not primitive)
        // 2. Not string, DateTime, Guid, etc.
        // 3. Could be ICollection<T>, List<T>, or direct entity reference
        if (propertyType.TypeKind == TypeKind.Class ||
            propertyType.TypeKind == TypeKind.Interface)
        {
            // It's a navigation property
            return true;
        }

        return false;
    }

    bool IsPrimaryKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;
        return name == "Id" ||
               name.EndsWith("Id") && name.Length > 2 &&
               char.IsUpper(name[name.Length - 3]); // e.g., EntityId, not ParentId
    }

    bool IsForeignKeyProperty(IPropertySymbol propertySymbol)
    {
        var name = propertySymbol.Name;
        var type = propertySymbol.Type;

        // Foreign keys are typically nullable or non-nullable integer/Guid types ending with "Id"
        if (!name.EndsWith("Id"))
        {
            return false;
        }

        // Check if the type is a scalar type suitable for FK
        return IsScalarType(type);
    }

    bool IsScalarType(ITypeSymbol type)
    {
        // Unwrap nullable
        if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            type = namedType.TypeArguments[0];
        }

        // Check for primitive and common scalar types
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
            case SpecialType.System_String:
            case SpecialType.System_Char:
            case SpecialType.System_DateTime:
                return true;
        }

        // Check for common value types like Guid, DateTimeOffset, TimeSpan
        var typeName = type.ToString();
        return typeName == "System.Guid" ||
               typeName == "System.DateTimeOffset" ||
               typeName == "System.TimeSpan" ||
               typeName == "System.DateOnly" ||
               typeName == "System.TimeOnly";
    }

    bool IsNestedPropertyAccess(MemberAccessExpressionSyntax memberAccess)
    {
        // Check if we're accessing a property on a property (e.g., Parent.Name)
        // The expression should be another MemberAccessExpressionSyntax
        return memberAccess.Expression is MemberAccessExpressionSyntax;
    }
}
