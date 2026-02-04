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
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var invocation = node as InvocationExpressionSyntax ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (invocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => ConvertToExplicitProjectionAsync(context.Document, invocation, c),
                equivalenceKey: title),
            diagnostic);
    }

    static async Task<Document> ConvertToExplicitProjectionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
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

        if (filterLambda == null)
        {
            return document;
        }

        var entityParamName = GetLastParameterName(filterLambda);
        if (entityParamName == null)
        {
            return document;
        }

        var accessedProperties = ExtractAccessedProperties(filterLambda.Body, entityParamName);
        if (accessedProperties.Count == 0)
        {
            return document;
        }

        var newProjectionLambda = BuildProjectionLambda(entityParamName, accessedProperties);
        var newFilterLambda = BuildFilterLambda(filterLambda, accessedProperties);

        // Replace arguments
        SyntaxNode newInvocation;
        if (projectionArgumentIndex == -1)
        {
            // 4-parameter syntax: add projection argument
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
            // Replace existing projection and filter arguments
            var newArguments = arguments;

            newArguments = newArguments.Replace(
                newArguments[projectionArgumentIndex],
                newArguments[projectionArgumentIndex].WithExpression(newProjectionLambda));

            newArguments = newArguments.Replace(
                newArguments[filterArgumentIndex],
                newArguments[filterArgumentIndex].WithExpression(newFilterLambda));

            newInvocation = invocation.WithArgumentList(
                invocation.ArgumentList.WithArguments(newArguments));
        }

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    static string? GetLastParameterName(LambdaExpressionSyntax lambda) =>
        lambda switch
        {
            SimpleLambdaExpressionSyntax simple => simple.Parameter.Identifier.Text,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: > 0 } parenthesized =>
                parenthesized.ParameterList.Parameters[parenthesized.ParameterList.Parameters.Count - 1].Identifier.Text,
            _ => null
        };

    static ExpressionSyntax UnwrapNullForgiving(ExpressionSyntax expression) =>
        expression is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.SuppressNullableWarningExpression } postfix
            ? postfix.Operand
            : expression;

    static List<PropertyAccess> ExtractAccessedProperties(CSharpSyntaxNode body, string paramName)
    {
        var properties = new List<PropertyAccess>();

        foreach (var memberAccess in body.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>())
        {
            // Look for: e.Nav.Prop or e.Nav!.Prop
            var inner = UnwrapNullForgiving(memberAccess.Expression);

            if (inner is not MemberAccessExpressionSyntax nestedAccess)
            {
                continue;
            }

            var root = UnwrapNullForgiving(nestedAccess.Expression);
            if (root is not IdentifierNameSyntax identifier || identifier.Identifier.Text != paramName)
            {
                continue;
            }

            var navName = nestedAccess.Name.Identifier.Text;
            var propName = memberAccess.Name.Identifier.Text;
            var fullPath = $"{navName}.{propName}";

            if (!properties.Any(_ => _.FullPath == fullPath))
            {
                properties.Add(new(fullPath, $"{navName}{propName}", memberAccess));
            }
        }

        return properties;
    }

    static LambdaExpressionSyntax BuildProjectionLambda(
        string paramName,
        List<PropertyAccess> properties)
    {
        var initializers = new List<AnonymousObjectMemberDeclaratorSyntax>
        {
            // Always include Id
            SyntaxFactory.AnonymousObjectMemberDeclarator(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(paramName),
                    SyntaxFactory.IdentifierName("Id")))
        };

        foreach (var prop in properties)
        {
            ExpressionSyntax expression = SyntaxFactory.IdentifierName(paramName);
            foreach (var part in prop.FullPath.Split('.'))
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

        return SyntaxFactory.SimpleLambdaExpression(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(paramName)),
            SyntaxFactory.AnonymousObjectCreationExpression(SyntaxFactory.SeparatedList(initializers)));
    }

    static LambdaExpressionSyntax BuildFilterLambda(
        LambdaExpressionSyntax originalFilter,
        List<PropertyAccess> properties)
    {
        // Replace all property accesses in a single pass
        var replacements = properties.ToDictionary(
            _ => _.OriginalAccess, SyntaxNode (_) =>
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("proj"),
                    SyntaxFactory.IdentifierName(_.FlatName)));

        var newBody = originalFilter.Body.ReplaceNodes(
            replacements.Keys,
            (original, _) => replacements[original]);

        if (originalFilter is ParenthesizedLambdaExpressionSyntax parenthesizedLambda)
        {
            var parameters = parenthesizedLambda.ParameterList.Parameters;
            var newParameters = parameters.RemoveAt(parameters.Count - 1)
                .Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("proj")));

            return SyntaxFactory.ParenthesizedLambdaExpression(
                SyntaxFactory.ParameterList(newParameters),
                newBody);
        }

        return SyntaxFactory.SimpleLambdaExpression(
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("proj")),
            newBody);
    }

    class PropertyAccess(string fullPath, string flatName, SyntaxNode originalAccess)
    {
        public string FullPath { get; } = fullPath;
        public string FlatName { get; } = flatName;
        public SyntaxNode OriginalAccess { get; } = originalAccess;
    }
}
