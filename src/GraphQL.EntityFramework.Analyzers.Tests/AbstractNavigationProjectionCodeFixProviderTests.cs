public class AbstractNavigationProjectionCodeFixProviderTests
{
    [Fact]
    public async Task ConvertsIdentityProjectionToExplicitProjection()
    {
        var source = """
            using GraphQL.EntityFramework;
            using Microsoft.EntityFrameworkCore;
            using System;

            public abstract class BaseEntity
            {
                public Guid Id { get; set; }
                public string Property { get; set; } = "";
            }

            public class ChildEntity
            {
                public Guid Id { get; set; }
                public BaseEntity? Parent { get; set; }
            }

            public class TestDbContext : DbContext { }

            public class TestClass
            {
                public void ConfigureFilters(Filters<TestDbContext> filters)
                {
                    filters.For<ChildEntity>().Add(
                        projection: _ => _,
                        filter: (_, _, _, c) => c.Parent!.Property == "test");
                }
            }
            """;

        var result = await ApplyCodeFixAsync(source);
        await Verify(result);
    }

    static async Task<string> ApplyCodeFixAsync(string source)
    {
        var document = CreateDocument(source);
        var compilation = await document.Project.GetCompilationAsync();
        if (compilation == null)
        {
            throw new("Compilation failed");
        }

        // Get diagnostics from analyzer
        var analyzer = new FilterIdentityProjectionAnalyzer();
        var compilationWithAnalyzers = compilation.WithAnalyzers([analyzer]);
        var allDiagnostics = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

        var diagnostic = allDiagnostics.FirstOrDefault(_ => _.Id == "GQLEF007");
        if (diagnostic == null)
        {
            throw new("No GQLEF007 diagnostic found");
        }

        // Apply code fix
        var codeFixProvider = new AbstractNavigationProjectionCodeFixProvider();
        var actions = new List<CodeAction>();

        var context = new CodeFixContext(
            document,
            diagnostic,
            (action, _) => actions.Add(action),
            default);

        await codeFixProvider.RegisterCodeFixesAsync(context);

        if (actions.Count == 0)
        {
            throw new($"No code fixes were registered. Diagnostic: {diagnostic}");
        }

        var operations = await actions[0].GetOperationsAsync(default);
        if (operations.IsEmpty)
        {
            throw new("Code action returned no operations");
        }

        var operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault();
        if (operation == null)
        {
            throw new("No ApplyChangesOperation found");
        }

        var changedSolution = operation.ChangedSolution;
        var changedDocument = changedSolution.GetDocument(document.Id);
        if (changedDocument == null)
        {
            throw new("Failed to get changed document");
        }

        var root = await changedDocument.GetSyntaxRootAsync();
        return root?.ToFullString() ?? "";
    }

    static Document CreateDocument(string source)
    {
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Filters<>).Assembly.Location),
        };

        var requiredAssemblies = new[]
        {
            typeof(Guid).Assembly,
            Assembly.Load("System.Runtime"),
            typeof(IQueryable<>).Assembly,
            Assembly.Load("System.Security.Claims"),
        };

        foreach (var assembly in requiredAssemblies)
        {
            if (!string.IsNullOrEmpty(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic &&
                !string.IsNullOrEmpty(assembly.Location) &&
                references.All(_ => _.Display != assembly.Location))
            {
                var name = assembly.GetName().Name ?? "";
                if ((!name.StartsWith("System.") &&
                     !name.StartsWith("Microsoft.")) ||
                    name.Contains("xunit", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
                catch
                {
                    // Ignore assemblies that can't be referenced
                }
            }
        }

        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReferences(projectId, references)
            .AddDocument(documentId, "TestDocument.cs", source);

        return solution.GetDocument(documentId)!;
    }
}
