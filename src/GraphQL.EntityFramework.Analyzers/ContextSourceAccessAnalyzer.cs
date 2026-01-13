using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GraphQL.EntityFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ContextSourceAccessAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ProblematicContextSourceAccess);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Check if we're in an EF graph type class
        if (!IsInEfGraphType(context))
        {
            return;
        }

        // Check if this is a field addition method
        if (!IsFieldAdditionMethod(invocation))
        {
            return;
        }

        // Extract and analyze lambda parameters
        AnalyzeLambdaArguments(context, invocation);
    }

    static bool IsInEfGraphType(SyntaxNodeAnalysisContext context)
    {
        var containingType = context.ContainingSymbol?.ContainingType;
        if (containingType == null)
        {
            return false;
        }

        // Check if inherits from EfObjectGraphType, EfInterfaceGraphType, or QueryGraphType
        var baseType = containingType.BaseType;
        while (baseType != null)
        {
            var baseTypeName = baseType.OriginalDefinition.ToDisplayString();
            if (baseTypeName.StartsWith("GraphQL.EntityFramework.EfObjectGraphType<") ||
                baseTypeName.StartsWith("GraphQL.EntityFramework.EfInterfaceGraphType<") ||
                baseTypeName.StartsWith("GraphQL.EntityFramework.QueryGraphType<"))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    static bool IsFieldAdditionMethod(InvocationExpressionSyntax invocation)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null)
        {
            return false;
        }

        var methodName = memberAccess.Name.Identifier.Text;

        return methodName is "AddNavigationField" or
            "AddNavigationListField" or
            "AddNavigationConnectionField" or
            "AddQueryField" or
            "AddQueryConnectionField" or
            "AddSingleField" or
            "AddFirstField";
    }

    static void AnalyzeLambdaArguments(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
    {
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            // Look for lambda expressions in resolve parameter
            if (argument.Expression is not SimpleLambdaExpressionSyntax and not ParenthesizedLambdaExpressionSyntax)
            {
                continue;
            }

            var lambdaBody = argument.Expression switch
            {
                SimpleLambdaExpressionSyntax simple => simple.Body,
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.Body,
                _ => null
            };

            if (lambdaBody == null)
            {
                continue;
            }

            // Get the parameter name (should be "context" typically)
            var parameterName = argument.Expression switch
            {
                SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
                ParenthesizedLambdaExpressionSyntax parenthesized =>
                    parenthesized.ParameterList.Parameters.FirstOrDefault()?.Identifier.Text,
                _ => null
            };

            if (parameterName == null)
            {
                continue;
            }

            // Walk the lambda body to find context.Source.PropertyName patterns
            var descendantNodes = lambdaBody.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
            foreach (var memberAccess in descendantNodes)
            {
                AnalyzeMemberAccess(context, memberAccess, parameterName);
            }
        }
    }

    static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context, MemberAccessExpressionSyntax memberAccess, string parameterName)
    {
        // Check if this is context.Source.PropertyName
        if (memberAccess.Expression is not MemberAccessExpressionSyntax innerMemberAccess)
        {
            return;
        }

        // Check if innerMemberAccess is context.Source
        if (innerMemberAccess.Expression is not IdentifierNameSyntax identifier ||
            identifier.Identifier.Text != parameterName ||
            innerMemberAccess.Name.Identifier.Text != "Source")
        {
            return;
        }

        // Now we have context.Source.PropertyName
        var propertyName = memberAccess.Name.Identifier.Text;

        // Skip if property is "Id" or ends with "Id" (foreign keys)
        if (propertyName.Equals("Id", System.StringComparison.OrdinalIgnoreCase) ||
            propertyName.EndsWith("Id", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Report diagnostic
        var foreignKeyHint = propertyName + "Id";
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.ProblematicContextSourceAccess,
            memberAccess.GetLocation(),
            propertyName,
            foreignKeyHint);

        context.ReportDiagnostic(diagnostic);
    }
}
