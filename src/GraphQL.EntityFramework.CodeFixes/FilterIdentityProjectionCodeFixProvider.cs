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

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the invocation expression - get the node directly from the span
        var node = root.FindNode(diagnosticSpan);
        var invocation = node as InvocationExpressionSyntax ??
                        node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();

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
        for (var i = 0; i < invocation.ArgumentList.Arguments.Count; i++)
        {
            if (invocation.ArgumentList.Arguments[i].NameColon?.Name.Identifier.Text == "projection")
            {
                projectionArgumentIndex = i;
                break;
            }
        }

        if (projectionArgumentIndex == -1)
        {
            return document;
        }

        // Use an annotation to track the invocation
        var annotation = new SyntaxAnnotation();
        var trackedRoot = root.ReplaceNode(invocation, invocation.WithAdditionalAnnotations(annotation));
        var trackedInvocation = trackedRoot.GetAnnotatedNodes(annotation).OfType<InvocationExpressionSyntax>().First();

        // Create new argument list without the projection argument
        var newArguments = trackedInvocation.ArgumentList.Arguments.RemoveAt(projectionArgumentIndex);
        var newArgumentList = trackedInvocation.ArgumentList.WithArguments(newArguments);

        var newInvocation = trackedInvocation.WithArgumentList(newArgumentList);
        var newRoot = trackedRoot.ReplaceNode(trackedInvocation, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }
}
