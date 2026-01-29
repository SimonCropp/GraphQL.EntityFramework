namespace GraphQL.EntityFramework.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FilterIdentityProjectionCodeFixProvider))]
[Shared]
public class FilterIdentityProjectionCodeFixProvider : CodeFixProvider
{
    const string Title = "Remove identity projection parameter";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [DiagnosticDescriptors.GQLEF004.Id];

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

        // Find the invocation expression - get the node directly from the span
        var node = root.FindNode(diagnosticSpan);
        var invocation = node as InvocationExpressionSyntax ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (invocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => RemoveIdentityProjectionAsync(context.Document, invocation, c),
                equivalenceKey: Title),
            diagnostic);
    }

    static async Task<Document> RemoveIdentityProjectionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root == null)
        {
            return document;
        }

        // Find the projection argument index
        var projectionArgumentIndex = -1;
        var arguments = invocation.ArgumentList.Arguments;
        for (var i = 0; i < arguments.Count; i++)
        {
            if (arguments[i].NameColon?.Name.Identifier.Text == "projection")
            {
                projectionArgumentIndex = i;
                break;
            }
        }

        if (projectionArgumentIndex == -1)
        {
            return document;
        }

        // Create new argument list without the projection argument
        var newArguments = arguments.RemoveAt(projectionArgumentIndex);
        var newArgumentList = invocation.ArgumentList.WithArguments(newArguments);
        var newInvocation = invocation.WithArgumentList(newArgumentList);

        // Replace the node in one operation
        var newRoot = root.ReplaceNode(invocation, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }
}
