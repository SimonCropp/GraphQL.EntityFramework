using Microsoft.CodeAnalysis.CSharp;

namespace GraphQL.EntityFramework.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AbstractNavigationProjectionCodeFixProvider))]
[Shared]
public class AbstractNavigationProjectionCodeFixProvider : CodeFixProvider
{
    const string title = "Convert to explicit projection";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [DiagnosticDescriptors.GQLEF007.Id];

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan);
        var invocation = node as InvocationExpressionSyntax ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (invocation == null)
        {
            return;
        }

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => ConvertToExplicitProjectionAsync(context.Document, invocation, semanticModel, c),
                equivalenceKey: title),
            diagnostic);
    }

    static async Task<Document> ConvertToExplicitProjectionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        Cancel cancel)
    {
        var root = await document.GetSyntaxRootAsync(cancel);
        if (root == null)
        {
            return document;
        }

        // Find projection and filter arguments
        var arguments = invocation.ArgumentList.Arguments;
        LambdaExpressionSyntax? filterLambda = null;
        var projectionArgumentIndex = -1;
        var filterArgumentIndex = -1;

        for (var i = 0; i < arguments.Count; i++)
        {
            var arg = arguments[i];
            if (arg.NameColon?.Name.Identifier.Text == "projection")
            {
                projectionArgumentIndex = i;
            }
            else if (arg.NameColon?.Name.Identifier.Text == "filter" &&
                     arg.Expression is LambdaExpressionSyntax filtLambda)
            {
                filterLambda = filtLambda;
                filterArgumentIndex = i;
            }
        }

        // If no explicit projection argument, this is 4-parameter syntax
        var is4ParamSyntax = projectionArgumentIndex == -1;

        if (filterLambda == null)
        {
            return document;
        }

        // Extract accessed properties from filter
        var accessedProperties = ExtractAccessedProperties(filterLambda, semanticModel);
        if (accessedProperties.Count == 0)
        {
            return document;
        }

        // Get the entity parameter name from the filter (last parameter)
        string entityParamName;
        if (filterLambda is SimpleLambdaExpressionSyntax simpleLambda)
        {
            entityParamName = simpleLambda.Parameter.Identifier.Text;
        }
        else if (filterLambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            var paramCount = parenthesizedLambda.ParameterList.Parameters.Count;
            entityParamName = paramCount > 0
                ? parenthesizedLambda.ParameterList.Parameters[paramCount - 1].Identifier.Text
                : "entity";
        }
        else
        {
            return document;
        }

        // Build new projection lambda: e => new { e.Id, Prop1 = e.Nav.Prop1, ... }
        var newProjectionLambda = BuildProjectionLambda(entityParamName, accessedProperties);

        // Build new filter lambda with renamed parameter: (_, _, _, proj) => proj.Prop1 == value
        var newFilterLambda = BuildFilterLambda(filterLambda, entityParamName, accessedProperties);

        // Replace arguments
        SyntaxNode newInvocation;
        if (is4ParamSyntax)
        {
            // Add projection argument
            var projectionArg = SyntaxFactory.Argument(newProjectionLambda)
                .WithNameColon(SyntaxFactory.NameColon("projection"))
                .WithLeadingTrivia(SyntaxFactory.Whitespace("\n            "));

            var filterArg = arguments[0].WithExpression(newFilterLambda);

            var newArguments = SyntaxFactory.SeparatedList([projectionArg, filterArg],
                [SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(SyntaxFactory.Whitespace(" "))]);

            newInvocation = invocation.WithArgumentList(
                invocation.ArgumentList.WithArguments(newArguments));
        }
        else
        {
            // Replace projection and filter arguments
            var newArguments = arguments;

            if (projectionArgumentIndex >= 0)
            {
                newArguments = newArguments.Replace(
                    newArguments[projectionArgumentIndex],
                    newArguments[projectionArgumentIndex].WithExpression(newProjectionLambda));
            }

            if (filterArgumentIndex >= 0)
            {
                newArguments = newArguments.Replace(
                    newArguments[filterArgumentIndex],
                    newArguments[filterArgumentIndex].WithExpression(newFilterLambda));
            }

            newInvocation = invocation.WithArgumentList(
                invocation.ArgumentList.WithArguments(newArguments));
        }

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    static List<PropertyAccess> ExtractAccessedProperties(
        LambdaExpressionSyntax filterLambda,
        SemanticModel semanticModel)
    {
        var properties = new List<PropertyAccess>();
        var body = filterLambda.Body;

        // Get filter parameter name
        string? paramName = null;
        if (filterLambda is SimpleLambdaExpressionSyntax simpleLambda)
        {
            paramName = simpleLambda.Parameter.Identifier.Text;
        }
        else if (filterLambda is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            var paramCount = parenthesizedLambda.ParameterList.Parameters.Count;
            if (paramCount > 0)
            {
                paramName = parenthesizedLambda.ParameterList.Parameters[paramCount - 1].Identifier.Text;
            }
        }

        if (paramName == null)
        {
            return properties;
        }

        // Find all member accesses
        var memberAccesses = body.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>();

        foreach (var memberAccess in memberAccesses)
        {
            // Look for patterns like: e.Parent.Property
            if (memberAccess.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } nestedAccess &&
                identifier.Identifier.Text == paramName)
            {
                // e.Parent.Property - extract as "Parent.Property"
                var navName = nestedAccess.Name.Identifier.Text;
                var propName = memberAccess.Name.Identifier.Text;
                var fullPath = $"{navName}.{propName}";
                var flatName = $"{navName}{propName}";

                if (!properties.Any(p => p.FullPath == fullPath))
                {
                    properties.Add(new(fullPath, flatName, memberAccess));
                }
            }
        }

        return properties;
    }

    static LambdaExpressionSyntax BuildProjectionLambda(
        string paramName,
        List<PropertyAccess> properties)
    {
        // Build: e => new { e.Id, Prop1 = e.Nav.Prop1, ... }
        var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(paramName));

        List<AnonymousObjectMemberDeclaratorSyntax> initializers = [];

        // Add Id property
        initializers.Add(
            SyntaxFactory.AnonymousObjectMemberDeclarator(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(paramName),
                    SyntaxFactory.IdentifierName("Id"))));

        // Add accessed properties with flattened names
        foreach (var prop in properties)
        {
            var parts = prop.FullPath.Split('.');
            ExpressionSyntax expression = SyntaxFactory.IdentifierName(paramName);

            foreach (var part in parts)
            {
                expression = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    SyntaxFactory.IdentifierName(part));
            }

            initializers.Add(
                SyntaxFactory.AnonymousObjectMemberDeclarator(
                    SyntaxFactory.NameEquals(prop.FlatName),
                    expression));
        }

        var anonymousObject = SyntaxFactory.AnonymousObjectCreationExpression(
            SyntaxFactory.SeparatedList(initializers));

        return SyntaxFactory.SimpleLambdaExpression(parameter, anonymousObject);
    }

    static LambdaExpressionSyntax BuildFilterLambda(
        LambdaExpressionSyntax originalFilter,
        string originalParamName,
        List<PropertyAccess> properties)
    {
        // Replace entity parameter references with proj and update property accesses
        var newBody = originalFilter.Body;

        // Replace each property access with flattened name
        foreach (var prop in properties)
        {
            // Replace e.Parent.Property with proj.ParentProperty
            newBody = newBody.ReplaceNodes(
                newBody.DescendantNodesAndSelf().Where(n => n == prop.OriginalAccess),
                (_, __) => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("proj"),
                    SyntaxFactory.IdentifierName(prop.FlatName)));
        }

        // Build new parameter list with "proj" as last parameter
        if (originalFilter is SimpleLambdaExpressionSyntax)
        {
            return SyntaxFactory.SimpleLambdaExpression(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("proj")),
                newBody);
        }
        else if (originalFilter is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            var parameters = parenthesizedLambda.ParameterList.Parameters;
            var newParameters = parameters.RemoveAt(parameters.Count - 1)
                .Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("proj")));

            return SyntaxFactory.ParenthesizedLambdaExpression(
                SyntaxFactory.ParameterList(newParameters),
                newBody);
        }

        return originalFilter;
    }

    class PropertyAccess
    {
        public PropertyAccess(string fullPath, string flatName, SyntaxNode originalAccess)
        {
            FullPath = fullPath;
            FlatName = flatName;
            OriginalAccess = originalAccess;
        }

        public string FullPath { get; }
        public string FlatName { get; }
        public SyntaxNode OriginalAccess { get; }
    }
}
